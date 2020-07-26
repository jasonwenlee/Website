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
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,KeyPointID,Importance,Description,DiagramURL,Header")] keypoint keypoint)
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
                if (keyPointInfo.Count() == 0)
                {
                    countSteps = 1;
                }
                else
                {
                    countSteps = keyPointInfo.Select(x => (int)x.Number).ToList().Max() + 1;
                }
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
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,KeyPointID,Importance,Description,DiagramURL,Header")] keypoint keypoint)
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
        public async Task<ActionResult> EditSequence(string keypoints, Dictionary<string, string> updates)
        {
            // USE TWO LISTS
            // Only send query to db to update and no need for server to send back updated list every time.
            // First list will be used to update on server.
            List<keypoint> listOfKeyPoints = new List<keypoint>();
            // Second list will be used to update list on the client-side display. No need to get new updated list from server.
            List<keypoint> passListToView = JsonConvert.DeserializeObject<List<keypoint>>(keypoints);
            
            // Initialise empty row to store our row to be updated
            keypoint rowNumber = new keypoint();
            foreach (KeyValuePair<string, string> item in updates)
            {
                if (int.TryParse(item.Key, out int key) && int.TryParse(item.Value, out int value))
                {
                    listOfKeyPoints = JsonConvert.DeserializeObject<List<keypoint>>(keypoints);
                    rowNumber = listOfKeyPoints.FirstOrDefault(x => x.Number == key);
                    rowNumber.Number = value;
                    HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, rowNumber.KeyPointID.ToString()), rowNumber);
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        passListToView.FirstOrDefault(x => x.KeyPointID == rowNumber.KeyPointID).Number = value;
                    }
                }
            }
            return Json(JsonConvert.SerializeObject(passListToView), JsonRequestBehavior.AllowGet);
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
