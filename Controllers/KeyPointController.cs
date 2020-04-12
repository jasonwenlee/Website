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
    public class KeyPointController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiKeyPointsPath"];

        // GET: KeyPoints
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<keypoint> keyPointInfo = new List<keypoint>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                keyPointInfo = JsonConvert.DeserializeObject<List<keypoint>>(responseDetail);
            }
            return View(keyPointInfo);
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson(int id)
        {
            List<keypoint> keyPointInfo = new List<keypoint>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                keyPointInfo = JsonConvert.DeserializeObject<List<keypoint>>(responseDetail).FindAll(x => x.ProcedureID == id);
                // Serialise back to json
                var keyPointInfoJson = JsonConvert.SerializeObject(keyPointInfo);
                return Json(keyPointInfoJson, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: KeyPoints/Details/
        // Note: similar to edit function but this will be used to get display information within the model
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            keypoint keyPointInfo = new keypoint();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                keyPointInfo = JsonConvert.DeserializeObject<keypoint>(responseDetail);
            }
            return View(keyPointInfo);
        }

        // GET: KeyPoints/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Procedure = ProcedureController.procName;
            return View();
        }

        // POST: KeyPoints/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,KeyPointID,Importance,Description,DiagramURL")] keypoint keypoint)
        {
            int countSteps = 0;
            List<keypoint> keyPointInfo = new List<keypoint>();
            // Send request to find web api REST service for steps
            HttpResponseMessage getResponse = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (getResponse.IsSuccessStatusCode)
            {
                var responseDetail = getResponse.Content.ReadAsStringAsync().Result;
                // Deserialise to find all steps belonging to chosen procedure
                keyPointInfo = JsonConvert.DeserializeObject<List<keypoint>>(responseDetail).FindAll(x => x.ProcedureID == ProcedureController.procID);
                countSteps = keyPointInfo.Select(x => (int)x.Number).ToList().Max() + 1;
            }

            keypoint.ProcedureID = ProcedureController.procID;
            keypoint.Number = countSteps;

            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, keypoint);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // GET: KeyPoints/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            keypoint keypointInfo = new keypoint();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                keypointInfo = JsonConvert.DeserializeObject<keypoint>(responseDetail);
            }
            return View(keypointInfo);
        }

        // POST: KeyPoints/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,KeyPointID,Importance,Description,DiagramURL")] keypoint keypoint)
        {
            keypoint.ProcedureID = ProcedureController.procID;
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, keypoint.KeyPointID.ToString()), keypoint);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Details", "Procedure", new { id = ProcedureController.procID });
        }

        // Edit sequence of rows and make updates to the database in server
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public async Task<ActionResult> EditSequence(string keypoints, string firstSequence, string secondSequence)
        {
            List<keypoint> listOfKeyPoints = JsonConvert.DeserializeObject<List<keypoint>>(keypoints);

            // Find rows that will be swapped
            keypoint findFirstRow = listOfKeyPoints.FirstOrDefault(x => x.Number == int.Parse(firstSequence));
            keypoint findSecondRow = listOfKeyPoints.FirstOrDefault(x => x.Number == int.Parse(secondSequence));

            // Proceed to swap rows
            findFirstRow.Number = int.Parse(secondSequence);
            findSecondRow.Number = int.Parse(firstSequence);

            HttpResponseMessage responseOne = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findFirstRow.KeyPointID.ToString()), findFirstRow);
            responseOne.EnsureSuccessStatusCode();
            HttpResponseMessage responseTwo = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, findSecondRow.KeyPointID.ToString()), findSecondRow);
            responseOne.EnsureSuccessStatusCode();

            if (responseOne.IsSuccessStatusCode && responseTwo.IsSuccessStatusCode)
            {
                // Get updated list again
                return Json(JsonConvert.SerializeObject(listOfKeyPoints), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: KeyPoints/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            ViewBag.Procedure = ProcedureController.procName;
            keypoint keyPointInfo = new keypoint();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                keyPointInfo = JsonConvert.DeserializeObject<keypoint>(responseDetail);
            }
            return View(keyPointInfo);
        }

        // POST: KeyPoints/Delete/
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
