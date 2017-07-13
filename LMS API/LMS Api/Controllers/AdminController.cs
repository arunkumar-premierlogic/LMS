﻿using LMS_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace LMS_Api.Controllers
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private Entities db = new Entities();

        //GET: api/LeaveRequests
        [HttpGet]
        [Route("LeaveRequests")]
        [ResponseType(typeof(List<appliedleave>))]
        public IHttpActionResult getLeaveRequests()
        {
            //var appliedleave = db.Leave_Approval_Matrix.Where(x => x.userId == userId).ToList();

            List<appliedleave> leaaverequests = (from k in db.Leave_Approval_Matrix
                                                 join lt in db.LeaveTypes on k.LeaveTypeId equals lt.Id
                                                 join u in db.Users on k.userId equals u.Id
                                                 //join ls in db.LeaveStatus on k.status equals ls.Id
                                                 join al in db.ApprovalLevels on k.LevelId equals al.Id
                                                 where k.status == 1 || k.status == 5 || k.status == 4
                                                 select new appliedleave
                                                 {
                                                     id = k.Id,
                                                     name = u.firstName + " " + u.lastName,
                                                     email = u.email,
                                                     date = k.startDate,
                                                     leavetype = lt.LeaveType1,
                                                     reason = k.reason,
                                                     status = k.status,
                                                     approverId = k.approverId,
                                                     //leaveStatus = ls.Status + " with " + al.Approver,
                                                     // action = 
                                                 }).ToList();



            if (leaaverequests == null)
            {
                return NotFound();
            }
            else
            {
                List<leaverequests> listofLeaveRequests = new List<Models.leaverequests>();
                foreach (var item in leaaverequests)
                {
                    leaverequests objleaveMatrix = new Models.leaverequests();
                    objleaveMatrix.id = item.id;
                    objleaveMatrix.name = item.name;
                    objleaveMatrix.email = item.email;
                    objleaveMatrix.startDate = item.date.ToString("dd-MM-yyyy");
                    objleaveMatrix.reason = item.reason;
                    objleaveMatrix.LeaveCategory = item.leavetype;
                    objleaveMatrix.status = item.status;
                    listofLeaveRequests.Add(objleaveMatrix);
                }

                return Ok(listofLeaveRequests);
            }
        }

        // GET: api/Users/5
        [HttpGet]
        [Route("LoadUpdateUserDetails/{empId}")]
        [ResponseType(typeof(User))]
        public IHttpActionResult LoadUpdateUserDetails(int empId)
        {
            try
            {
                User user = db.Users.Where(x => x.Id == empId).FirstOrDefault();
                if (user == null)
                {
                    return NotFound();
                }
                else
                {
                    UserModel usermodel = new UserModel();
                    usermodel.id = user.Id;
                    usermodel.username = user.email;
                    usermodel.firstName = user.firstName;
                    usermodel.designationId = user.designationTypeId;
                    usermodel.lastName = user.lastName;
                    usermodel.reportingToId = user.reportingToUserId;
                    return Ok(usermodel);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }


        // GET: api/Users/5
        [HttpGet]
        [Route("DeleteUser/{empId}")]
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int empId)
        {
            try
            {
                db.Users.Where(x => x.Id == empId).FirstOrDefault().IsDeleted = true;
                db.SaveChanges();
                return Ok("true");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        //Leaveapproved
        [HttpPost]
        [Route("ApprovedLeave")]
        public IHttpActionResult LeaveApproved(leaveapproved objApprovedLeaveMatrix)
        {
            try
            {
                //Leave_Approval_Matrix objLeave_Approval_Matrix = new Leave_Approval_Matrix();
                var approvedleave = db.Leave_Approval_Matrix.Where(x => x.Id == objApprovedLeaveMatrix.id).FirstOrDefault();
                if (approvedleave == null)
                {
                    return NotFound();
                }
                else
                {
                    //approvedleave.Remarks = objApprovedLeaveMatrix.remarks;
                    if (objApprovedLeaveMatrix.status == 3 || objApprovedLeaveMatrix.status == 2)
                        approvedleave.status = objApprovedLeaveMatrix.status;

                    //adding approver comments to remarks table
                    LeaveRemark objLeaveRemark = new LeaveRemark();
                    objLeaveRemark.LeaveId = objApprovedLeaveMatrix.id;
                    objLeaveRemark.remarks = objApprovedLeaveMatrix.remarks;
                    db.LeaveRemarks.Add(objLeaveRemark);

                    db.SaveChanges();
                    return Ok("Saved");
                }
            }
            catch (Exception e)
            {
                return Ok(e.InnerException);

            }
        }

        [HttpPost]
        [Route("UpdateAdminSettings")]
        public IHttpActionResult UpdateAdminSettings(adminsettings objadminsettings)
        {
            try
            {
                db.AdminSettings.Where(x => x.Id == objadminsettings.id).FirstOrDefault().SettingValue = objadminsettings.IsAdminPermissionRequiredForLeaveApproval;
                db.SaveChanges();

                return Ok("Updated");
            }
            catch (Exception e)
            {
                return Ok(e.InnerException);

            }
        }

        [HttpGet]
        [Route("LoadAdminSettings")]
        public IHttpActionResult GetAdminSetting()
        {
            try
            {
                return Ok(db.AdminSettings.ToList());
            }
            catch (Exception e)
            {
                return Ok(e.InnerException);

            }
        }

        [HttpGet]
        [Route("LoadUsersForAdmin")]
        public IHttpActionResult LoadUsersForAdmin()
        {
            try
            {
                var users = db.Users.Where(x => x.IsDeleted != true).ToList();

                List<Models.UserModel> listofUsers = new List<Models.UserModel>();
                foreach (var item in users)
                {
                    UserModel objuser = new UserModel();
                    objuser.id = item.Id;
                    objuser.firstName = item.firstName;
                    objuser.lastName = item.lastName;
                    objuser.username = item.email;
                    listofUsers.Add(objuser);
                }

                return Ok(listofUsers);
            }
            catch (Exception e)
            {
                return Ok(e.InnerException);

            }
        }

        [HttpGet]
        [Route("ReportingLeads")]
        [ResponseType(typeof(List<User>))]
        public IHttpActionResult ReportingLeads()
        {
            var ReportingLeads = db.Users.Where(x => x.designationTypeId != 4 && x.designationTypeId != 5).ToList();
            if (ReportingLeads == null)
            {
                return NotFound();
            }
            else
            {
                List<Models.UserModel> listofReportingLeads = new List<Models.UserModel>();
                foreach (var item in ReportingLeads)
                {
                    UserModel objuser = new UserModel();
                    objuser.id = item.Id;
                    objuser.firstName = item.firstName;
                    objuser.lastName = item.lastName;
                    listofReportingLeads.Add(objuser);
                }

                return Ok(listofReportingLeads);
            }
        }

        [HttpGet]
        [Route("Designations")]
        [ResponseType(typeof(List<User>))]
        public IHttpActionResult Designation()
        {
            var designationList = db.Designations.ToList();
            if (designationList == null)
            {
                return NotFound();
            }
            else
            {
                List<Designation> listofDesignation = new List<Designation>();
                foreach (var item in designationList)
                {
                    Designation objDesignation = new Designation();
                    objDesignation.Id = item.Id;
                    objDesignation.designationType = item.designationType;

                    listofDesignation.Add(objDesignation);
                }

                return Ok(listofDesignation);
            }
        }

        //Register User
        [HttpPost]
        [Route("RegisterUser")]
        [ResponseType(typeof(string))]
        public IHttpActionResult RegisterUser(UserModel objUser)
        {
            try
            {
                var user = db.Users.FirstOrDefault(x => x.email == objUser.username);

                if (user == null)
                {

                    User newUser = new User();
                    newUser.firstName = objUser.firstName;
                    newUser.lastName = objUser.lastName;
                    newUser.email = objUser.username;
                    newUser.password = objUser.password;
                    newUser.designationTypeId = objUser.designationId;
                    newUser.reportingToUserId = objUser.reportingToId;
                    newUser.DateOfJoining = objUser.DateOfJoining;
                    newUser.Gender = objUser.Gender[0].ToString();
                    newUser.createdDate = DateTime.Now;
                    newUser.updatedDate = DateTime.Now;

                    db.Users.Add(newUser);

                    db.SaveChanges();
                    return Ok(true);
                }
                else
                {
                    return Ok("User Already Exists");
                }
            }
            catch (Exception e)
            {
                return Ok(e.InnerException);

            }
        }

        //Update User
        [HttpPost]
        [Route("UpdateUser")]
        [ResponseType(typeof(string))]
        public IHttpActionResult UpdateUser(UserModel objUser)
        {
            try
            {
                var user = db.Users.FirstOrDefault(x => x.email == objUser.username);

                if (user != null)
                {
                    user.firstName = objUser.firstName;
                    user.lastName = objUser.lastName;
                    //user.email = objUser.username;
                    //user.password = objUser.password;
                    user.designationTypeId = objUser.designationId;
                    user.reportingToUserId = objUser.reportingToId;
                    //userer.createdDate = DateTime.Now;
                    user.updatedDate = DateTime.Now;

                    db.SaveChanges();
                    return Ok(true);
                }
                else
                {
                    return Ok("User does not exists");
                }
            }
            catch (Exception e)
            {
                return Ok(e.InnerException);

            }
        }

        [HttpPost]
        [Route("UploadFileApi")]
        [ResponseType(typeof(HttpResponseMessage))]
        public HttpResponseMessage UploadJsonFile()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var abc = Request.Properties.Values;
            var httpRequest = HttpContext.Current.Request;
            var fileCount = httpRequest.Files.Count;
            if (httpRequest.Files.Count > 0)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    var postedFile = httpRequest.Files[i];
                    var filePath = HttpContext.Current.Server.MapPath("~/UploadFile/" + postedFile.FileName);
                    postedFile.SaveAs(filePath);
                }
            }
            return response;

            //HttpResponseMessage response = new HttpResponseMessage();
            //var httpRequest = HttpContext.Current.Request;
            //if (httpRequest.Files.Count > 0)
            //{
            //    foreach (string file in httpRequest.Files)
            //    {
            //        var postedFile = httpRequest.Files[file];
            //        var filePath = HttpContext.Current.Server.MapPath("~/UploadFile/" + postedFile.FileName);
            //        postedFile.SaveAs(filePath);
            //    }   
            //}
            //return response;
        }
    }
}
