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
    public class ComplicationController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiComplicationsPath"];

        // GET: Complications
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<complication> complicationInfo = new List<complication>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                complicationInfo = JsonConvert.DeserializeObject<List<complication>>(responseDetail);
            }
            return View(complicationInfo);
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson(int id)
        {
            List<complication> complicationInfo = new List<complication>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                complicationInfo = JsonConvert.DeserializeObject<List<complication>>(responseDetail).FindAll(x => x.ProcedureID == id);
                // Serialise back to json
                var complicationInfoJson = JsonConvert.SerializeObject(complicationInfo);
                return Json(complicationInfoJson, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Complications/Details/
        // Note: similar to edit function but this will be used to get display information within the model
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            complication complicationInfo = new complication();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                complicationInfo = JsonConvert.DeserializeObject<complication>(responseDetail);
            }
            return View(complicationInfo);
        }

        // GET: Complications/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Procedure = ProcedureController.procName;
            return View();
        }

        // POST: Complications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,ComplicationID,Name,DiagramURL")] complication complication)
        {
            int countSteps = 0;
            List<complication> complicationInfo = new List<complication>();
            // Send request to find web api REST service for steps
            HttpResponseMessage getResponse = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (getResponse.IsSuccessStatusCode)
            {
                var responseDetail = getResponse.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                complicationInfo = JsonConvert.DeserializeObject<List<complication>>(responseDetail).FindAll(x => x.ProcedureID == ProcedureController.procID);
                countSteps = complicationInfo.Select(x => (int)x.Number).ToList().Max() + 1;
            }

            complication.ProcedureID = ProcedureController.procID;
            complication.Number = countSteps;

            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, complication);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // GET: Complications/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            complication complicationInfo = new complication();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                complicationInfo = JsonConvert.DeserializeObject<complication>(responseDetail);
            }
            return View(complicationInfo);
        }

        // POST: Complications/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,ComplicationID,Name,DiagramURL")] complication complication)
        {
            complication.ProcedureID = ProcedureController.procID;
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, complication.ComplicationID.ToString()), complication);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // Edit sequence of rows and make updates to the database in server
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public async Task<ActionResult> EditSequence(string complications, string firstSequence, string secondSequence)
        {
            List<complication> listOfComplication = JsonConvert.DeserializeObject<List<complication>>(complications);

            // Find rows that will be swapped
            complication findFirstRow = listOfComplication.FirstOrDefault(x => x.Number == int.Parse(firstSequence));
            complication findSecondRow = listOfComplication.FirstOrDefault(x => x.Number == int.Parse(secondSequence));

            // Proceed to swap rows
            findFirstRow.Number = int.Parse(secondSequence);
            findSecondRow.Number = int.Parse(firstSequence);

            HttpResponseMessage responseOne = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findFirstRow.ComplicationID.ToString()), findFirstRow);
            responseOne.EnsureSuccessStatusCode();
            HttpResponseMessage responseTwo = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findSecondRow.ComplicationID.ToString()), findSecondRow);
            responseTwo.EnsureSuccessStatusCode();

            if (responseOne.IsSuccessStatusCode && responseTwo.IsSuccessStatusCode)
            {
                // Get updated list again
                return Json(JsonConvert.SerializeObject(listOfComplication), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Complications/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            complication complicationInfo = new complication();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                complicationInfo = JsonConvert.DeserializeObject<complication>(responseDetail);
            }
            return View(complicationInfo);
        }

        // POST: Complications/Delete/
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