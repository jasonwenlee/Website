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
    public class ReferenceController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiReferencesPath"];

        // GET: References
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<reference> referenceInfo = new List<reference>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                referenceInfo = JsonConvert.DeserializeObject<List<reference>>(responseDetail);
            }
            return View(referenceInfo);
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson(int id)
        {
            List<reference> referenceInfo = new List<reference>();

            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                referenceInfo = JsonConvert.DeserializeObject<List<reference>>(responseDetail).FindAll(x => x.ProcedureID == id);
                // Serialise back to json
                var referenceInfoJson = JsonConvert.SerializeObject(referenceInfo);
                return Json(referenceInfoJson, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: References/Details/
        // Note: similar to edit function but this will be used to get display information within the model
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            reference referenceInfo = new reference();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                referenceInfo = JsonConvert.DeserializeObject<reference>(responseDetail);
            }
            return View(referenceInfo);
        }

        // GET: References/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Procedure = ProcedureController.procName;
            return View();
        }

        // POST: Reference/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,ReferenceID,Content")] reference reference)
        {
            int countSteps = 0;
            List<reference> referenceInfo = new List<reference>();
            // Send request to find web api REST service for steps
            HttpResponseMessage getResponse = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (getResponse.IsSuccessStatusCode)
            {
                var responseDetail = getResponse.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                referenceInfo = JsonConvert.DeserializeObject<List<reference>>(responseDetail).FindAll(x => x.ProcedureID == ProcedureController.procID);
                countSteps = referenceInfo.Select(x => (int)x.Number).ToList().Max() + 1;
            }

            reference.ProcedureID = ProcedureController.procID;
            reference.Number = countSteps;

            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, reference);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // GET: References/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            reference referenceInfo = new reference();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                referenceInfo = JsonConvert.DeserializeObject<reference>(responseDetail);
            }
            return View(referenceInfo);
        }

        // POST: References/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,ReferenceID,Content")] reference reference)
        {
            reference.ProcedureID = ProcedureController.procID;
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, reference.ReferenceID.ToString()), reference);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // Edit sequence of rows and make updates to the database in server
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public async Task<ActionResult> EditSequence(string references, string newSequence, string oldSequence)
        {
            var newSeq = int.Parse(newSequence) + 1;
            var oldSeq = int.Parse(oldSequence) + 1;
            List<reference> referenceInfo = JsonConvert.DeserializeObject<List<reference>>(references);

            // Find rows that will be swapped
            reference findFirstRow = referenceInfo.FirstOrDefault(x => x.Number == oldSeq);
            reference findSecondRow = referenceInfo.FirstOrDefault(x => x.Number == newSeq);

            // Proceed to swap rows
            findFirstRow.Number = newSeq;
            findSecondRow.Number = oldSeq;

            HttpResponseMessage responseOne = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findFirstRow.ReferenceID.ToString()), findFirstRow);
            responseOne.EnsureSuccessStatusCode();
            HttpResponseMessage responseTwo = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findSecondRow.ReferenceID.ToString()), findSecondRow);
            responseOne.EnsureSuccessStatusCode();
            return null;
        }

        // GET: References/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            reference referenceInfo = new reference();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                referenceInfo = JsonConvert.DeserializeObject<reference>(responseDetail);
            }
            return View(referenceInfo);
        }

        // POST: References/Delete/
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