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
    public class VariationController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiVariationsPath"];

        // GET: Variations
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<variation> variationInfo = new List<variation>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                variationInfo = JsonConvert.DeserializeObject<List<variation>>(responseDetail);
            }
            return View(variationInfo);
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson(int id)
        {
            List<variation> variationInfo = new List<variation>();

            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                variationInfo = JsonConvert.DeserializeObject<List<variation>>(responseDetail).FindAll(x => x.ProcedureID == id);
                // Serialise back to json
                var variationInfoJson = JsonConvert.SerializeObject(variationInfo);
                return Json(variationInfoJson, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Variations/Details/
        // Note: similar to edit function but this will be used to get display information within the model
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            variation variationInfo = new variation();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                variationInfo = JsonConvert.DeserializeObject<variation>(responseDetail);
            }
            return View(variationInfo);
        }

        // GET: Variations/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Procedure = ProcedureController.procName;
            return View();
        }

        // POST: Variations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,VariationID,Header,SubHeader")] variation variation)
        {
            int countSteps = 0;
            List<variation> variationInfo = new List<variation>();
            // Send request to find web api REST service for steps
            HttpResponseMessage getResponse = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (getResponse.IsSuccessStatusCode)
            {
                var responseDetail = getResponse.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                variationInfo = JsonConvert.DeserializeObject<List<variation>>(responseDetail).FindAll(x => x.ProcedureID == ProcedureController.procID);
                countSteps = variationInfo.Select(x => (int)x.Number).ToList().Max() + 1;
            }

            variation.ProcedureID = ProcedureController.procID;
            variation.Number = countSteps;

            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, variation);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // GET: Variations/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            variation variationInfo = new variation();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                variationInfo = JsonConvert.DeserializeObject<variation>(responseDetail);
            }
            return View(variationInfo);
        }

        // POST: Variations/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,VariationID,Header,SubHeader")] variation variation)
        {
            variation.ProcedureID = ProcedureController.procID;
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, variation.VariationID.ToString()), variation);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // Edit sequence of rows and make updates to the database in server
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public async Task<ActionResult> EditSequence(string variations, string firstSequence, string secondSequence)
        {
            List<variation> listOfVariations = JsonConvert.DeserializeObject<List<variation>>(variations);

            // Find rows that will be swapped
            variation findFirstRow = listOfVariations.FirstOrDefault(x => x.Number == int.Parse(firstSequence));
            variation findSecondRow = listOfVariations.FirstOrDefault(x => x.Number == int.Parse(secondSequence));

            // Proceed to swap rows
            findFirstRow.Number = int.Parse(secondSequence);
            findSecondRow.Number = int.Parse(firstSequence);

            HttpResponseMessage responseOne = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findFirstRow.VariationID.ToString()), findFirstRow);
            responseOne.EnsureSuccessStatusCode();
            HttpResponseMessage responseTwo = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findSecondRow.VariationID.ToString()), findSecondRow);
            responseOne.EnsureSuccessStatusCode();

            if (responseOne.IsSuccessStatusCode && responseTwo.IsSuccessStatusCode)
            {
                // Get updated list again
                return Json(JsonConvert.SerializeObject(listOfVariations), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Variations/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            variation variationInfo = new variation();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                variationInfo = JsonConvert.DeserializeObject<variation>(responseDetail);
            }
            return View(variationInfo);
        }

        // POST: Variations/Delete/
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