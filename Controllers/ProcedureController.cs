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
    public class ProcedureController : Controller
    {
        HttpClient client = MvcApplication.httpClient;
        string urlPath = WebConfigurationManager.AppSettings["apiProceduresPath"];
        public static int procID;
        public static string procName;

        // GET: Procedure
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            List<procedure> procInfo = new List<procedure>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                procInfo = JsonConvert.DeserializeObject<List<procedure>>(responseDetail);
                return View(procInfo);
            }
            return null;
        }

        // Get index in Json format
        [HttpGet]
        public async Task<ActionResult> IndexJson()
        {
            List<procedure> procInfo = new List<procedure>();
            // Send request to find web api REST service
            HttpResponseMessage response = await client.GetAsync(urlPath);

            // Check if response is successful 
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                return Json(responseDetail, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Procedure/Details/
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            procedure procInfo = new procedure();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));

            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                procInfo = JsonConvert.DeserializeObject<procedure>(responseDetail);
                procID = (int)procInfo.ProcedureID;
                procName = procInfo.LongName;
                return View(procInfo); 
            }
            return null;
        }

        // Get procedure details in Json format
        [HttpGet]
        public async Task<ActionResult> DetailsJson(int? id)
        {
            procedure procInfo = new procedure();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));

            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                return Json(responseDetail, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: Procedure/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Procedure/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProcedureID,LongName,ShortName,VideoSource,Description")] procedure procedure)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(urlPath, procedure);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Index");
        }

        // GET: Procedure/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            procedure procInfo = new procedure();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                procInfo = JsonConvert.DeserializeObject<procedure>(responseDetail);
            }
            return View(procInfo);
        }

        // POST: Procedure/Edit/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProcedureID,LongName,ShortName,VideoSource,Description")] procedure procedure)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(String.Format("{0}/{1}", urlPath, procedure.ProcedureID.ToString()), procedure);
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Index");
        }

        // GET: Procedure/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            procedure procInfo = new procedure();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HttpResponseMessage response = await client.GetAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseDetail = response.Content.ReadAsStringAsync().Result;
                procInfo = JsonConvert.DeserializeObject<procedure>(responseDetail);
            }
            return View(procInfo);
        }

        // POST: Procedure/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage response = await client.DeleteAsync(String.Format("{0}/{1}", urlPath, id.ToString()));
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Index");
        }

    }
}