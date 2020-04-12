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
    public class HistoryController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiHistoriesPath"];

        // GET: History
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<history> historyInfo = new List<history>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                historyInfo = JsonConvert.DeserializeObject<List<history>>(responseDetail);
            }
            return View(historyInfo);
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson(int id)
        {
            List<history> historyInfo = new List<history>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                historyInfo = JsonConvert.DeserializeObject<List<history>>(responseDetail).FindAll(x => x.ProcedureID == id);
                // Serialise back to json
                var historyInfoJson = JsonConvert.SerializeObject(historyInfo);
                return Json(historyInfoJson, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: History/Details/
        // Note: similar to edit function but this will be used to get display information within the model
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            history historyInfo = new history();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                historyInfo = JsonConvert.DeserializeObject<history>(responseDetail);
            }
            return View(historyInfo);
        }

        // GET: History/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Procedure = ProcedureController.procName;
            return View();
        }

        // POST: History/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,HistoryID,Content")] history history)
        {
            int countSteps = 0;
            List<history> historyInfo = new List<history>();
            // Send request to find web api REST service for steps
            HttpResponseMessage getResponse = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (getResponse.IsSuccessStatusCode)
            {
                var responseDetail = getResponse.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                historyInfo = JsonConvert.DeserializeObject<List<history>>(responseDetail).FindAll(x => x.ProcedureID == ProcedureController.procID);
                countSteps = historyInfo.Select(x => (int)x.Number).ToList().Max() + 1;
            }

            history.ProcedureID = ProcedureController.procID;
            history.Number = countSteps;

            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, history);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // GET: History/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            history historyInfo = new history();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                historyInfo = JsonConvert.DeserializeObject<history>(responseDetail);
            }
            return View(historyInfo);
        }

        // POST: History/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,HistoryID,Content")] history history)
        {
            history.ProcedureID = ProcedureController.procID;
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, history.HistoryID.ToString()), history);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // Edit sequence of rows and make updates to the database in server
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public async Task<ActionResult> EditSequence(string history, string firstSequence, string secondSequence)
        {
            // Get list of history as objects
            List<history> listOfHistory = JsonConvert.DeserializeObject<List<history>>(history);

            // Find rows that will be swapped
            history findFirstRow = listOfHistory.FirstOrDefault(x => x.Number == int.Parse(firstSequence));
            history findSecondRow = listOfHistory.FirstOrDefault(x => x.Number == int.Parse(secondSequence));

            // Proceed to swap rows
            findFirstRow.Number = int.Parse(secondSequence);
            findSecondRow.Number = int.Parse(firstSequence);

            HttpResponseMessage responseOne = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findFirstRow.HistoryID.ToString()), findFirstRow);
            responseOne.EnsureSuccessStatusCode();
            HttpResponseMessage responseTwo = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findSecondRow.HistoryID.ToString()), findSecondRow);
            responseOne.EnsureSuccessStatusCode();

            if (responseOne.IsSuccessStatusCode && responseTwo.IsSuccessStatusCode)
            {
                // Get updated list again
                return Json(JsonConvert.SerializeObject(listOfHistory), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: History/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            history historyInfo = new history();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                historyInfo = JsonConvert.DeserializeObject<history>(responseDetail);
            }
            return View(historyInfo);
        }

        // POST: History/Delete/
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