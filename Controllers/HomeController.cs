using DataSets___DataTables.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Data;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace DataSets___DataTables.Controllers
{
    public class HomeController : Controller
    {
        public static readonly DataTable _datatable = new();
        public void InitialiseDB()
        {
            if (_datatable.Columns.Count == 0)
            {
                _datatable.Columns.Add("ID", typeof(string));
                _datatable.Columns.Add("FirstName", typeof(string));
                _datatable.Columns.Add("LastName", typeof(string));
                _datatable.Columns.Add("Position", typeof(string));
                _datatable.Columns.Add("Salary", typeof(int));
                _datatable.PrimaryKey = new DataColumn[] { _datatable.Columns["ID"] };
            }
        }
       
        public IActionResult Index()
        {
            List<EmployeeModel> AllEmployees = new();
            foreach (DataRow row in _datatable.Rows)
            {
                EmployeeModel employee = new()
                { 
                  Id = row["ID"].ToString(),
                  FirstName = row["FirstName"].ToString(),
                  LastName = row["LastName"].ToString(),
                  Position = row["Position"].ToString(),
                  Salary = (int) row["Salary"]
                };

                AllEmployees.Add(employee);
            }

            return View(AllEmployees);
        }



        // CREATE
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(string FName, string LName, string Pos, int Salary)
        {
            if (_datatable.Columns.Count != 0)
            {
                string id = Guid.NewGuid().ToString();
                _datatable.Rows.Add(id, FName, LName, Pos, Salary);
                return RedirectToAction("Index");
            }
            else
            {
                return BadRequest("NO AUTHORIZATION");
            }
        }

        //DELETE
        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Delete(string DeleteID)
        {
            for (int IntegerCounter = _datatable.Rows.Count - 1;  IntegerCounter >= 0; IntegerCounter--)
            {
                DataRow CurrentRow = _datatable.Rows[IntegerCounter];
                string CurrentID = CurrentRow["ID"].ToString();

                if (CurrentID == DeleteID)
                {
                    CurrentRow.Delete();
                    return Ok("Operation Successfull: " + CurrentID + " has been deleted" );
                }
            }
            _datatable.AcceptChanges();
            return BadRequest("No Data Found With This ID");
        }

        // EDIT
        [HttpPost]
        public IActionResult Edit(EmployeeModel EditEmployee)
        {
            DataRow EditRow = _datatable.Rows.Find(EditEmployee.Id);
            if (EditRow != null)
            {
                EditRow["FirstName"] = EditEmployee.FirstName;
                EditRow["LastName"] = EditEmployee.LastName;
                EditRow["Position"] = EditEmployee.Position;
                EditRow["Salary"] = EditEmployee.Salary;
            }
            return RedirectToAction("Index");
            
        }

        [Route("/webhook")]
        [HttpPost]
        public IActionResult WebhookHandler([FromBody] JsonObject payload)
        {
            try
            {
                PayloadModel WebhookPayload = JsonSerializer.Deserialize<PayloadModel>(payload);
                if (WebhookPayload.Authorization == "TopSecretKey")
                {
                    InitialiseDB();
                }
                else
                {
                    return BadRequest("Invalid Authorization");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Incorrect Format or Data");
            }
            
            return Ok("Authorized");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
