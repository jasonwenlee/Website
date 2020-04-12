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
        public async Task<ActionResult> EditSequence(string steps, string firstSequence, string secondSequence)
        {
            List<step> listOfStep = JsonConvert.DeserializeObject<List<step>>(steps);
            // Find rows that will be swapped
            step findFirstRow = listOfStep.FirstOrDefault(x => x.Number == int.Parse(firstSequence));
            step findSecondRow = listOfStep.FirstOrDefault(x => x.Number == int.Parse(secondSequence));

            // Proceed to swap rows
            findFirstRow.Number = int.Parse(secondSequence);
            findSecondRow.Number = int.Parse(firstSequence);

            HttpResponseMessage responseOne = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findFirstRow.StepID.ToString()), findFirstRow);
            responseOne.EnsureSuccessStatusCode();
            HttpResponseMessage responseTwo = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findSecondRow.StepID.ToString()), findSecondRow);
            responseTwo.EnsureSuccessStatusCode();

            if (responseOne.IsSuccessStatusCode && responseTwo.IsSuccessStatusCode)
            {
                // Get updated list again
                return Json(JsonConvert.SerializeObject(listOfStep), JsonRequestBehavior.AllowGet);
            }
            return null;
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
