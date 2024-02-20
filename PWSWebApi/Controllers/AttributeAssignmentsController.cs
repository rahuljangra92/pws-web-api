using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PWSWebApi.handlers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace PWSWebApi.Controllers
{
    [Route("aa")]
    [ApiController]
    public class AttributeAssignmentsController : ControllerBase
    {
        private readonly string connectionString;
        private readonly Helper handlerHelper;

        public AttributeAssignmentsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            handlerHelper = new Helper(configuration);
        }


        [HttpPost]
        [Route("attribassignsearch")]
        public IActionResult AttribAssignSearch(string UserID, string AttribTypeIDs, string AttribNameIDs, string UserGroupIDs, string PWSRefIDs, string FnctTypeIDs, string DPIDs, string LoadSourceIDs, string AttribTypeValueMin, string AttribTypeValueMax, string AttribMinValueMin, string AttribMinValueMax, string AttribMaxValueMin, string AttribMaxValueMax, string AttribStartDateStart, string AttribStartDateEnd, string AttribEndDateStart, string AttribEndDateEnd, int NumRows = 30)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@NumRows", NumRows);
                if (!Helper.IgnoreAsNullParameter(AttribTypeIDs))
                    pars.Add("@AttribTypeIDs", AttribTypeIDs);
                if (!Helper.IgnoreAsNullParameter(AttribNameIDs))
                    pars.Add("@AttribNameIDs", AttribNameIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(PWSRefIDs))
                    pars.Add("@PWSRefIDs", PWSRefIDs);
                if (!Helper.IgnoreAsNullParameter(FnctTypeIDs))
                    pars.Add("@FnctTypeIDs", FnctTypeIDs);
                if (!Helper.IgnoreAsNullParameter(DPIDs))
                    pars.Add("@DPIDs", DPIDs);
                if (!Helper.IgnoreAsNullParameter(LoadSourceIDs))
                    pars.Add("@LoadSourceIDs", LoadSourceIDs);
                if (!Helper.IgnoreAsNullParameter(AttribTypeValueMin))
                    pars.Add("@AttribTypeValueMin", AttribTypeValueMin);
                if (!Helper.IgnoreAsNullParameter(AttribTypeValueMax))
                    pars.Add("@AttribTypeValueMax", AttribTypeValueMax);
                if (!Helper.IgnoreAsNullParameter(AttribMinValueMin))
                    pars.Add("@AttribMinValueMin", AttribMinValueMin);
                if (!Helper.IgnoreAsNullParameter(AttribMinValueMax))
                    pars.Add("@AttribMinValueMax", AttribMinValueMax);
                if (!Helper.IgnoreAsNullParameter(AttribMaxValueMin))
                    pars.Add("@AttribMaxValueMin", AttribMaxValueMin);
                if (!Helper.IgnoreAsNullParameter(AttribMaxValueMax))
                    pars.Add("@AttribMaxValueMax", AttribMaxValueMax);
                if (!Helper.IgnoreAsNullParameter(AttribStartDateStart))
                    pars.Add("@AttribStartDateStart", AttribStartDateStart);
                if (!Helper.IgnoreAsNullParameter(AttribStartDateEnd))
                    pars.Add("@AttribStartDateEnd", AttribStartDateEnd);
                if (!Helper.IgnoreAsNullParameter(AttribEndDateStart))
                    pars.Add("@AttribEndDateStart", AttribEndDateStart);
                if (!Helper.IgnoreAsNullParameter(AttribEndDateEnd))
                    pars.Add("@AttribEndDateEnd", AttribEndDateEnd);
                var data = Helper.callProcedure("[adm].[usp_AttribAssign_Search]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("attribassigninsert")]
        public IActionResult AttribAssignInsert(string UserID, string AttribTypeID, string AttribNameID, string UserGroupID, string PWSRefID, string FnctTypeID, string DPID, string LoadSourceID, string AttribTypeValue, string AttribMinValue, string AttribMaxValue, string AttribStartDate, string AttribEndDate, string Angular)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Angular", Angular);
                // pars.Add("@NumRows", NumRows);
                if (!Helper.IgnoreAsNullParameter(AttribTypeID))
                    pars.Add("@AttribTypeID", AttribTypeID);
                if (!Helper.IgnoreAsNullParameter(AttribNameID))
                    pars.Add("@AttribNameID", AttribNameID);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(PWSRefID))
                    pars.Add("@PWSRefID", PWSRefID);
                if (!Helper.IgnoreAsNullParameter(FnctTypeID))
                    pars.Add("@FnctTypeID", FnctTypeID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(AttribTypeValue))
                    pars.Add("@AttribTypeValue", AttribTypeValue);
                if (!Helper.IgnoreAsNullParameter(AttribMinValue))
                    pars.Add("@AttribMinValue", AttribMinValue);
                if (!Helper.IgnoreAsNullParameter(AttribMaxValue))
                    pars.Add("@AttribMaxValue", AttribMaxValue);
                if (!Helper.IgnoreAsNullParameter(AttribStartDate))
                    pars.Add("@AttribStartDate", AttribStartDate);
                if (!Helper.IgnoreAsNullParameter(AttribEndDate))
                    pars.Add("@AttribEndDate", AttribEndDate);
                var data = Helper.callProcedure("[adm].[usp_AttribAssign_Insert]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("attribassigndelete")]
        public IActionResult AttributeAssignDelete(int UserID, string AttribAssignID, int Angular = 0)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AttribAssignID", AttribAssignID);
                pars.Add("@Angular", Angular);
                var data = Helper.callProcedure("[adm].[usp_AttribAssign_Delete]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("attribAssignedit")]
        public IActionResult AttribAssignEdit(int UserID, int AttribAssignID, int AttribTypeID, int AttribNameID, int UserGroupID, int PWSRefID, int FnctTypeID, int DPID, int LoadSourceID, string AttribTypeValue, string AttribMinValue, string AttribMaxValue, string AttribStartDate, string AttribEndDate, string SortOrder, int Angular = 0)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AttribAssignID", AttribAssignID);
                pars.Add("@AttribTypeID", AttribTypeID);
                pars.Add("@AttribNameID", AttribNameID);
                pars.Add("@UserGroupID", UserGroupID);
                pars.Add("@PWSRefID", PWSRefID);
                pars.Add("@FnctTypeID", FnctTypeID);
                pars.Add("@DPID", DPID);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@AttribStartDate", AttribStartDate);
                pars.Add("@AttribEndDate", AttribEndDate);
                pars.Add("@Angular", Angular);
                if (!Helper.IgnoreAsNullParameter(AttribMinValue))
                    pars.Add("@AttribMinValue", AttribMinValue);
                if (!Helper.IgnoreAsNullParameter(AttribMaxValue))
                    pars.Add("@AttribMaxValue", AttribMaxValue);
                if (!Helper.IgnoreAsNullParameter(SortOrder))
                    pars.Add("@SortOrder", SortOrder);
                if (!Helper.IgnoreAsNullParameter(AttribTypeValue))
                    pars.Add("@AttribTypeValue", AttribTypeValue);
                var data = Helper.callProcedure("[adm].[usp_AttribAssign_Edit]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }
    }
}