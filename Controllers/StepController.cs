using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Website.Models;

namespace Website.Controllers
{
    public class StepController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiStepsPath"];

        // GET: Step
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<step> stepInfo = new List<step>();
            // Send request to find web api REST service for steps
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                stepInfo = JsonConvert.DeserializeObject<List<step>>(responseDetail);
            }
            return View(stepInfo);
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson(int id)
        {
            List<step> stepInfo = new List<step>();
            // Send request to find web api REST service for steps
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                stepInfo = JsonConvert.DeserializeObject<List<step>>(responseDetail).FindAll(x => x.ProcedureID == id);
                // Serialise back to json
                var stepInfoJson = JsonConvert.SerializeObject(stepInfo);
                return Json(stepInfoJson, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Step/Details/
        // Note: similar to edit function but this will be used to get display information within the model
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            step stepInfo = new step();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                stepInfo = JsonConvert.DeserializeObject<step>(responseDetail);
            }
            return View(stepInfo);
        }

        // GET: Step/Create
        public ActionResult Create()
        {
            ViewBag.Procedure = ProcedureController.procName;
            return View();
        }

        // POST: Step/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,StepID,Content,DiagramURL,Number")] step step)
        {
            int countSteps = 0;
            List<int> listOfNumbers = new List<int>();
            List<step> stepInfo = new List<step>();
            // Send request to find web api REST service for steps
            HttpResponseMessage getResponse = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (getResponse.IsSuccessStatusCode)
            {
                var responseDetail = getResponse.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                stepInfo = JsonConvert.DeserializeObject<List<step>>(responseDetail).FindAll(x => x.ProcedureID == ProcedureController.procID);
                countSteps = stepInfo.Select(x => (int)x.Number).ToList().Max() + 1;
            }

            step.ProcedureID = ProcedureController.procID;
            step.Number = countSteps;

            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, step);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details","Procedure", new { id = ProcedureController.procID });
        }

        // GET: Step/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            step stepInfo = new step();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                stepInfo = JsonConvert.DeserializeObject<step>(responseDetail);
            }
            return View(stepInfo);
        }

        // POST: Step/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,StepID,Content,DiagramURL,Number")] step step)
        {
            step.ProcedureID = ProcedureController.procID;
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, step.StepID.ToString()), step);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // Edit sequence of rows and make updates to the database in server
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public async Task<ActionResult> EditSequence(string steps, Dictionary<string, string> updates)
        {
            // USE TWO LISTS
            // Only send query to db to update and no need for server to send back updated list every time.
            // First list will be used to update on server.
            List<step> listOfSteps = new List<step>();
            // Second list will be used to update list on the client-side display. No need to get new updated list from server.
            List<step> passListToView = JsonConvert.DeserializeObject<List<step>>(steps);

            // Initialise empty row to store our row to be updated
            step rowNumber = new step();
            foreach (KeyValuePair<string, string> item in updates)
            {
                listOfSteps = JsonConvert.DeserializeObject<List<step>>(steps);
                rowNumber = listOfSteps.FirstOrDefault(x => x.Number == int.Parse(item.Key));
                rowNumber.Number = int.Parse(item.Value);
                HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, rowNumber.StepID.ToString()), rowNumber);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    passListToView.FirstOrDefault(x => x.StepID == rowNumber.StepID).Number = int.Parse(item.Value);
                }
            }
            return Json(JsonConvert.SerializeObject(passListToView), JsonRequestBehavior.AllowGet);
        }

        // GET: Step/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            step stepInfo = new step();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                stepInfo = JsonConvert.DeserializeObject<step>(responseDetail);
            }
            return View(stepInfo);
        }

        // POST: Step/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage response = await client.DeleteAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

    }
}
