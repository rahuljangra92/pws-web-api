using Microsoft.Extensions.Configuration;
using PWSWebApi.Controllers;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.Domains;
using PWSWebApi.ViewModel.Models;
using PWSWebApi.ViewModel.Models.Operations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PWSWebApi.ViewModel.Helper
{
    public class HelperOperations
    {
        //string connectionString = ConfigurationManager.ConnectionStrings["PWSProd"].ToString();
        //string connectionStringPwsRec = ConfigurationManager.ConnectionStrings["PWSRec"].ToString();
        private readonly string connectionString;
        private readonly string connectionStringPwsRec;

        public HelperOperations(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            connectionStringPwsRec = configuration.GetConnectionString("PWSRec");
        }


        public List<ResponseError> UIValidatePerfStatic(string gridColumnsID, string userId, List<ColumnsFromExcel> cols, List<PerfStatic> perfStatics)
        {
            List<ResponseError> errors = new List<ResponseError>();
            var _pwsContext = new DataClasses1DataContext(connectionString);
            var _pwsRecContext = new DataClasses1DataContext(connectionStringPwsRec);
            var gridColumnDetails = _pwsContext.GridColumnsDetails.Where(f => f.GridColumnsID == Convert.ToInt32(gridColumnsID)).OrderBy(f => f.ColumnOrder).AsQueryable().ToList();
            var columnTitles = gridColumnDetails.Select(f => f.ColumnTitle).Distinct().ToList();

            var colIndex = 0;
            var uiid = 0;

            foreach (var item in perfStatics)
            {
                Validate(gridColumnDetails[colIndex], errors, item.PerfAttribDescr, uiid, item, perfStatics);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.PerfAttribID, uiid, item, perfStatics);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.BeginDate, uiid, item, perfStatics);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.EndDate, uiid, item, perfStatics);
                colIndex += 1;

                item.BMV = string.IsNullOrEmpty(item.BMV) ? "" : item.BMV.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.BMV, uiid, item, perfStatics);
                colIndex += 1;

                item.EMV = string.IsNullOrEmpty(item.EMV) ? "" : item.EMV.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.EMV, uiid, item, perfStatics);
                colIndex += 1;

                item.BegFlow = string.IsNullOrEmpty(item.BegFlow) ? "" : item.BegFlow.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.BegFlow, uiid, item, perfStatics);
                colIndex += 1;

                item.NetFlow = string.IsNullOrEmpty(item.NetFlow) ? "" : item.NetFlow.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.NetFlow, uiid, item, perfStatics);
                colIndex += 1;

                item.MgtFee = string.IsNullOrEmpty(item.MgtFee) ? "" : item.MgtFee.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.MgtFee, uiid, item, perfStatics);
                colIndex += 1;

                item.GRet = string.IsNullOrEmpty(item.GRet) ? "" : item.GRet.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.GRet, uiid, item, perfStatics);
                if (!string.IsNullOrEmpty(item.GRet))
                {
                    if (Convert.ToDouble(item.GRet) < -100)
                    {
                        errors.Add(new ResponseError
                        {
                            Column = gridColumnDetails[colIndex].ColumnTitle,
                            ErrorMessage = "This field has to be greater than -100",
                            UIId = uiid.ToString()
                        });
                    }
                }
                colIndex += 1;

                item.NRet = string.IsNullOrEmpty(item.NRet) ? "" : item.NRet.Replace(",", string.Empty);
                Validate(gridColumnDetails[colIndex], errors, item.NRet, uiid, item);
                if (!string.IsNullOrEmpty(item.NRet))
                {
                    if (Convert.ToDouble(item.NRet) < -100)
                    {
                        errors.Add(new ResponseError
                        {
                            Column = gridColumnDetails[colIndex].ColumnTitle,
                            ErrorMessage = "This field has to be greater than -100",
                            UIId = uiid.ToString()
                        });
                    }
                }
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Stat, uiid, item);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.DPID, uiid, item);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.LoadSourceID, uiid, item);

                colIndex = 0;

                uiid++;
            }

            return errors;
        }

        public List<ResponseError> UIValidateRptModel(string gridColumnsID, string userId, List<ColumnsFromExcel> cols, List<ReportModel> reportModels)
        {
            List<ResponseError> errors = new List<ResponseError>();
            var _pwsContext = new DataClasses1DataContext(connectionString);
            var _pwsRecContext = new DataClasses1DataContext(connectionStringPwsRec);
            var gridColumnDetails = _pwsContext.GridColumnsDetails.Where(f => f.GridColumnsID == Convert.ToInt32(gridColumnsID)).AsQueryable().ToList();
            var columnTitles = gridColumnDetails.Select(f => f.ColumnTitle).Distinct().ToList();

            var colIndex = 0;
            var uiid = 0;

            foreach (var item in reportModels)
            {
                Validate(gridColumnDetails[colIndex], errors, item.RowOrder, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.ParentRowOrder, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Levels, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.ModelTitle, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Description, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AttribTypeID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AttribNameID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AttribOrder, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.UserGroupID, uiid);
                colIndex = 0;

                uiid++;
            }

            return errors;
        }

        public List<ResponseError> UIValidateTrans(string gridColumnsID, string userId, List<ColumnsFromExcel> cols, List<Trans> trans)
        {
            List<ResponseError> errors = new List<ResponseError>();
            var _pwsContext = new DataClasses1DataContext(connectionString);
            var _pwsRecContext = new DataClasses1DataContext(connectionStringPwsRec);
            var gridColumnDetails = _pwsContext.GridColumnsDetails.Where(f => f.GridColumnsID == Convert.ToInt32(gridColumnsID)).AsQueryable().ToList();
            var columnTitles = gridColumnDetails.Select(f => f.ColumnTitle).Distinct().ToList();

            var colIndex = 0;
            var uiid = 0;

            foreach (var item in trans)
            {
                Validate(gridColumnDetails[colIndex], errors, item.TransID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.TransProcessCodeID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.ProcessOrd, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.StaticData, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.DPTransID, uiid);
                colIndex += 1; //5


                Validate(gridColumnDetails[colIndex], errors, item.DPLotID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AcctID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.SubAcctID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.ReliefMethod, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.SecID, uiid);
                colIndex += 1; // 10


                Validate(gridColumnDetails[colIndex], errors, item.TCodeID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.DPTransCodeName, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.EffectiveDate, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.TradeDate, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.SettleDate, uiid);
                colIndex += 1; // 15


                Validate(gridColumnDetails[colIndex], errors, item.AcquisitionTradeDate, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AcquisitionSettleDate, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Shrs, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.OFShrs, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Price, uiid);
                colIndex += 1; // 20


                Validate(gridColumnDetails[colIndex], errors, item.PriceMult, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.TradeCCYID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.SettleCCYID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.BaseCCYID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.TradeFXRate, uiid);
                colIndex += 1; // 25


                Validate(gridColumnDetails[colIndex], errors, item.SettleFXRate, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.BaseFXRate, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.GrossLocal, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.GrossBase, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Fees, uiid);
                colIndex += 1; // 30


                Validate(gridColumnDetails[colIndex], errors, item.ForeignWithhold, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.NetLocal, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.NetBase, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AILocal, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AIBase, uiid);
                colIndex += 1; // 35


                Validate(gridColumnDetails[colIndex], errors, item.AALocal, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.AABase, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.OrigCostLocal, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.OrigCostBase, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.BookCostLocal, uiid);
                colIndex += 1; // 40


                Validate(gridColumnDetails[colIndex], errors, item.BookCostBase, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.RecordCostLocal, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.RecordCostBase, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.PurYld, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Comment, uiid);
                colIndex += 1; // 45


                Validate(gridColumnDetails[colIndex], errors, item.Final, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.Stat, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.LoadSourceID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.DPID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.OriginalTransID, uiid);
                colIndex += 1; // 50


                Validate(gridColumnDetails[colIndex], errors, item.TransTypeID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.TransSubTypeID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.LinkAcctID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.LinkSubAcctID, uiid);
                colIndex += 1;

                Validate(gridColumnDetails[colIndex], errors, item.UserDefined1, uiid);
                colIndex += 1; // 55

                Validate(gridColumnDetails[colIndex], errors, item.UserDefined2, uiid);
                colIndex = 0;

                uiid++;
            }

            return errors;
        }


        public List<ResponseError> UIValidateNewAccounts(string gridColumnsID, string userId, List<ColumnsFromExcel> cols, List<NewAccount> newAccts)
        {
            List<ResponseError> errors = new List<ResponseError>();
            var _pwsContext = new DataClasses1DataContext(connectionString);
            var gridColumnDetails = _pwsContext.GridColumnsDetails.Where(f => f.GridColumnsID == Convert.ToInt32(gridColumnsID)).AsQueryable().ToList();
            var columnTitles = gridColumnDetails.Select(f => f.ColumnTitle).Distinct().ToList();

            var colIndex = 0;
            var uiid = 0;
            foreach (var item in newAccts)
            {
                Validate(gridColumnDetails[colIndex], errors, item.AcctCode, uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, item.AcctNum, uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, item.AcctName, uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, item.AcctNickNamePrimary, uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, item.BaseCurr, uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.LoadSourceID), uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.UserGroupID), uiid);
                colIndex += 1;



                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.ReliefMethodID), uiid);
                colIndex += 1;



                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.EntityCode), uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.OwnedPct), uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.StartingDate), uiid);
                colIndex += 1;


                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.EndingDate), uiid);
                colIndex += 1;



                Validate(gridColumnDetails[colIndex], errors, Convert.ToString(item.PortfolioCode), uiid);
                colIndex = 0;

                uiid++;
            }

            return errors;
        }





        private void Validate(GridColumnsDetail colDetail, List<ResponseError> errors, string value, int i, PerfStatic perfItem = null, List<PerfStatic> perfStatics = null)
        {

            var _pwsContext = new DataClasses1DataContext(connectionString);
            if (colDetail.ColumnTitle.Equals("PerfAttribID", StringComparison.OrdinalIgnoreCase))
            {

                if (!string.IsNullOrEmpty(value))
                {
                    int num;
                    bool isNum = int.TryParse(value, out num);

                    if (isNum)
                    {
                        //check if the record exists in the PerfAttrib table, because it has to exist there in order to add this to temp table
                        var entryExistencesWithSamePerfAttribId = _pwsContext.PerfAttribs.SingleOrDefault(f => f.PerfAttribID == num);

                        if (entryExistencesWithSamePerfAttribId == null) // generate error so the pertattribid cannot be added
                        {
                            errors.Add(new ResponseError
                            {
                                Column = colDetail.ColumnTitle,
                                ErrorMessage = "Performance Attribute ID is invalid because it does not exist in the system",
                                UIId = i.ToString()
                            });
                        }
                    }
                }
            }


            if (perfItem != null && (!string.IsNullOrEmpty(perfItem.BeginDate) && !string.IsNullOrEmpty(perfItem.EndDate)) &&
                (colDetail.ColumnTitle.Equals("begindate", StringComparison.OrdinalIgnoreCase) ||
                  colDetail.ColumnTitle.Equals("enddate", StringComparison.OrdinalIgnoreCase)
                )
               )
            {

                DateTime date;
                bool isDate = DateTime.TryParse(perfItem.BeginDate, out date);
                var beginDate = !isDate ? Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(perfItem.BeginDate))) : Convert.ToDateTime(perfItem.BeginDate);
                var endDate = !isDate ? Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(perfItem.EndDate))) : Convert.ToDateTime(perfItem.EndDate);

                if (beginDate >= endDate)
                {
                    errors.Add(new ResponseError
                    {
                        Column = colDetail.ColumnTitle,
                        ErrorMessage = "Begin date must be earlier than the End date",
                        UIId = i.ToString()
                    });

                    return;
                }
                else
                {
                    if (perfItem != null && perfStatics != null && perfStatics[0].BeginDateFormatted != DateTime.MinValue)
                    {
                        var recordsWithSamePerfAttibId = perfStatics.Where(f => f.PerfAttribID == perfItem.PerfAttribID).ToList();

                        var beginDateConflicts = recordsWithSamePerfAttibId.Where(f => beginDate >= f.BeginDateFormatted.AddDays(-1) &&
                                                                                     beginDate < f.EndDateFormatted
                                                                                ).ToList();
                        if (beginDateConflicts.Count > 1 && colDetail.ColumnTitle.Equals("begindate", StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add(new ResponseError
                            {
                                Column = colDetail.ColumnTitle,
                                ErrorMessage = "Begin Date is conflicting with other date ranges in the excel file",
                                UIId = i.ToString()
                            });
                        }


                        var endDateConflicts = recordsWithSamePerfAttibId.Where(f => endDate > f.BeginDateFormatted.AddDays(-1) &&
                                                                                     endDate <= f.EndDateFormatted
                                                                                ).ToList();

                        if (endDateConflicts.Count > 1 && colDetail.ColumnTitle.Equals("enddate", StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add(new ResponseError
                            {
                                Column = colDetail.ColumnTitle,
                                ErrorMessage = "End Date is conflicting with other date ranges in the excel file",
                                UIId = i.ToString()
                            });
                        }
                    }
                }
            }

            if (colDetail.ColumnTitle.Equals("Stat", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var isValidEntry = value.Equals("R", StringComparison.OrdinalIgnoreCase) || value.Equals("A", StringComparison.OrdinalIgnoreCase);
                    if (!isValidEntry)
                    {
                        errors.Add(new ResponseError
                        {
                            Column = colDetail.ColumnTitle,
                            ErrorMessage = "This field must have a value of either A or R",
                            UIId = i.ToString()
                        });

                        return;
                    }
                }
            }

            bool requiredForRetired = false;
            if (perfItem != null)
            {
                requiredForRetired = (colDetail.ColumnTitle.Equals("PerfAttribID", StringComparison.OrdinalIgnoreCase) ||
                                         colDetail.ColumnTitle.Equals("BeginDate", StringComparison.OrdinalIgnoreCase) ||
                                         colDetail.ColumnTitle.Equals("EndDate", StringComparison.OrdinalIgnoreCase)
                                      );
            }


            if (Convert.ToInt32(colDetail.Req) == 1)
            {
                if (perfItem != null && perfItem.Stat.ToUpper() == "R" && !requiredForRetired)
                {
                    // for Stat = "R" only perfattibid and the dates are required
                }
                else if (string.IsNullOrEmpty(value))
                {
                    errors.Add(new ResponseError
                    {
                        Column = colDetail.ColumnTitle,
                        ErrorMessage = "This field is required",
                        UIId = i.ToString()
                    });

                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return; // if not required and empty pr null value, no need to validate this
                }
            }

            if (value == null)
            {
                value = "";
            }
            else
            {
                value = value.Trim();
            }





            var dataType = colDetail.Datatype.Trim().ToLower();

            if (!dataType.Equals("varchar"))
            {
                if (dataType.Equals("int"))
                {
                    if (value.Contains("."))
                    {
                        errors.Add(new ResponseError
                        {
                            Column = colDetail.ColumnTitle,
                            ErrorMessage = "This field needs to be an integer",
                            UIId = i.ToString()
                        });

                        return;
                    }

                    int val;
                    if (!int.TryParse(value, out val))
                    {
                        errors.Add(new ResponseError
                        {
                            Column = colDetail.ColumnTitle,
                            ErrorMessage = "This field needs to be an integer",
                            UIId = i.ToString()
                        });

                        return;
                    }
                }


                if (dataType.Equals("decimal"))
                {
                    decimal val;
                    if (!decimal.TryParse(value, out val))
                    {
                        errors.Add(new ResponseError
                        {
                            Column = colDetail.ColumnTitle,
                            ErrorMessage = "This field needs to be a number with zero to " + colDetail.Decimals + " decimal places",
                            UIId = i.ToString()
                        });

                        return;
                    }
                    else if (value.ToString().Contains("."))
                    {
                        var decimalPorstion = Convert.ToString(value).Split('.')[1];

                        if (decimalPorstion.Count() > colDetail.Decimals)
                        {
                            errors.Add(new ResponseError
                            {
                                Column = colDetail.ColumnTitle,
                                ErrorMessage = "This field needs to be a number with a maximum of " + colDetail.Decimals + " places",
                                UIId = i.ToString()
                            });
                        }

                        return;
                    }
                }

                if (dataType.Equals("date") || dataType.Equals("datetime"))
                {
                    DateTime val;
                    if (value.Trim().Length == 5)
                    {
                        var d = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(value)));
                        if (!DateTime.TryParse(d.ToShortDateString(), out val))
                        {
                            errors.Add(new ResponseError
                            {
                                Column = colDetail.ColumnTitle,
                                ErrorMessage = "This field needs to a valid date with format mm/dd/yyyy",
                                UIId = i.ToString()
                            });

                            return;
                        }

                    }



                    else if (!DateTime.TryParse(value, out val))
                    {
                        errors.Add(new ResponseError
                        {
                            Column = colDetail.ColumnTitle,
                            ErrorMessage = "This field needs to a valid date with format mm/dd/yyyy",
                            UIId = i.ToString()
                        });

                        return;
                    }
                }

                if (dataType.Equals("bit"))
                {
                    if (Convert.ToString(value) == "y" || Convert.ToString(value) == "n" || Convert.ToString(value) == "0" || Convert.ToString(value) == "1")
                    {
                        // all good
                    }
                    else
                    {
                        errors.Add(new ResponseError
                        {
                            Column = colDetail.ColumnTitle,
                            ErrorMessage = "Pleaas type y or 1 to indicate yes or type n or 0 to indicate no",
                            UIId = i.ToString()
                        });

                        return;
                    }
                }
            }

            if (value.Length > colDetail.MaxLen)
            {
                errors.Add(new ResponseError
                {
                    Column = colDetail.ColumnTitle,
                    ErrorMessage = "The length of this field cannot exceed " + colDetail.MaxLen,
                    UIId = i.ToString()
                });

                return;
            }

            if (value.Length < colDetail.MinLen && dataType != "date" && value.Length >= 5)
            {
                errors.Add(new ResponseError
                {
                    Column = colDetail.ColumnTitle,
                    ErrorMessage = "The minimum length of this field is " + colDetail.MinLen,
                    UIId = i.ToString()
                });

                return;
            }
        }


        //        //public List<ResponseError> UIValidateTrans(List<List<string>> operations, string gridColumnsID, string userId, List<ColumnsFromExcel> cols, List<New_Accounts_tmp> newAccts)
        //        //{
        //        //    var _pwsContext = new DataClasses1DataContext(connectionString);
        //        //    var gridColumnDetails = _pwsContext.GridColumnsDetails.Where(f => f.GridColumnsID == Convert.ToInt32(gridColumnsID)).AsQueryable().ToList();
        //        //    var columnTitles = gridColumnDetails.Select(f => f.ColumnTitle).Distinct().ToList();
        //        //    operations = operations.ToList();

        //        //    List<ResponseError> errors = new List<ResponseError>();
        //        //    try
        //        //    {
        //        //        for (int i = 1; i < operations.Count; i++)
        //        //        {
        //        //            var rowArray = operations[i];

        //        //            for (int j = 0; j < rowArray.Count; j++)
        //        //            {
        //        //                var value = rowArray[j];
        //        //                var cellPosition = cols[j].name + (i+1).ToString();
        //        //                var colDetail = gridColumnDetails[j];

        //        //                if (Convert.ToInt32(colDetail.Req) == 1)
        //        //                {
        //        //                    if (string.IsNullOrEmpty(value))
        //        //                    {
        //        //                        errors.Add(new ResponseError
        //        //                        {
        //        //                            Column = colDetail.ColumnTitle,
        //        //                            ErrorMessage = " ": This field is required",
        //        //                            UIId = i.ToString()
        //        //                        });

        //        //                        continue;
        //        //                    }
        //        //                }
        //        //                else
        //        //                {
        //        //                    if (string.IsNullOrEmpty(value))
        //        //                    {
        //        //                        continue; // if not required and empty pr null value, no need to validate this
        //        //                    }
        //        //                }

        //        //                value = value.Trim();


        //        //                if (value.Length > colDetail.MaxLen)
        //        //                {
        //        //                    errors.Add(new ResponseError
        //        //                    {
        //        //                        Column = colDetail.ColumnTitle,
        //        //                        ErrorMessage = " ": The length of this field cannot exceed " + colDetail.MaxLen,
        //        //                        UIId = i.ToString()
        //        //                    });

        //        //                    continue;
        //        //                }

        //        //                if (value.Length < colDetail.MinLen)
        //        //                {
        //        //                    errors.Add(new ResponseError
        //        //                    {
        //        //                        Column = colDetail.ColumnTitle,
        //        //                        ErrorMessage = " ": The minimum length of this field is " + colDetail.MinLen,
        //        //                        UIId = i.ToString()
        //        //                    });

        //        //                    continue;
        //        //                }

        //        //                var dataType = colDetail.Datatype.Trim().ToLower();

        //        //                if (!dataType.Equals("varchar"))
        //        //                {
        //        //                    if (dataType.Equals("int"))
        //        //                    {
        //        //                        if (value.Contains("."))
        //        //                        {
        //        //                            errors.Add(new ResponseError
        //        //                            {
        //        //                                Column = colDetail.ColumnTitle,
        //        //                                ErrorMessage = " ": This field needs to be an integer",
        //        //                                UIId = i.ToString()
        //        //                            });

        //        //                            continue;
        //        //                        }

        //        //                        int val;
        //        //                        if (!int.TryParse(value, out val))
        //        //                        {
        //        //                            errors.Add(new ResponseError
        //        //                            {
        //        //                                Column = colDetail.ColumnTitle,
        //        //                                ErrorMessage = " ": This field needs to be an integer",
        //        //                                UIId = i.ToString()
        //        //                            });

        //        //                            continue;
        //        //                        }
        //        //                    }


        //        //                    if (dataType.Equals("decimal"))
        //        //                    {
        //        //                        decimal val;
        //        //                        if (!decimal.TryParse(value, out val))
        //        //                        {
        //        //                            errors.Add(new ResponseError
        //        //                            {
        //        //                                Column = colDetail.ColumnTitle,
        //        //                                ErrorMessage = " ": This field needs to be a number with zero or " + colDetail.Decimals + " decimal places",
        //        //                                UIId = i.ToString()
        //        //                            });

        //        //                            continue;
        //        //                        }
        //        //                        else if(value.ToString().Contains("."))
        //        //                        {
        //        //                            var decimalPorstion = Convert.ToString(value).Split('.')[1];

        //        //                            if (decimalPorstion.Count() > colDetail.Decimals)
        //        //                            {
        //        //                                errors.Add(new ResponseError
        //        //                                {
        //        //                                    Column = colDetail.ColumnTitle,
        //        //                                    ErrorMessage = " ": This field needs to be a number with a maximum of " + colDetail.Decimals + "places",
        //        //                                    UIId = i.ToString()
        //        //                                });
        //        //                            }

        //        //                            continue;
        //        //                        }
        //        //                    }

        //        //                    if (dataType.Equals("date") || dataType.Equals("datetime"))
        //        //                    {
        //        //                        DateTime val;

        //        //                        if (!DateTime.TryParse(value, out val))
        //        //                        {
        //        //                            errors.Add(new ResponseError
        //        //                            {
        //        //                                Column = colDetail.ColumnTitle,
        //        //                                ErrorMessage = " ": This field needs to a valid date with format mm/dd/yyyy",
        //        //                                UIId = i.ToString()
        //        //                            });

        //        //                            continue;
        //        //                        }
        //        //                    }

        //        //                    if (dataType.Equals("bit"))
        //        //                    {
        //        //                        if (Convert.ToString(value) == "y" || Convert.ToString(value) == "n")
        //        //                        {
        //        //                            // all good
        //        //                        }
        //        //                        else
        //        //                        {
        //        //                            errors.Add(new ResponseError
        //        //                            {
        //        //                                Column = colDetail.ColumnTitle,
        //        //                                ErrorMessage = " ": Pleaas type y for yes or type n for no",
        //        //                                UIId = i.ToString()
        //        //                            });

        //        //                            continue;
        //        //                        }
        //        //                    }
        //        //                }
        //        //            }
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {

        //        //    }

        //        //    return errors;
        //        //}

        //        public string SaveTempGridCopy(List<List<string>> operations, string gridColumnsID, string userId)
        //        {
        //            try
        //            {
        //                operations = operations.Skip(1).ToList();
        //                var _contextPWS = new DataClasses1DataContext(connectionString);
        //                var _contextPWSRec = new PWSRecDataContext(connectionStringPwsRec);

        //                if (gridColumnsID == "3") // Trans
        //                {
        //                    var allDataForUser = _contextPWSRec.NewEditTrans_tmps.Where(f => f.UserID.Equals(userId)).AsQueryable();
        //                    _contextPWSRec.NewEditTrans_tmps.DeleteAllOnSubmit(allDataForUser);

        //                    List<NewEditTrans_tmp> itemsToBeSaved = new List<NewEditTrans_tmp>();

        //                    operations.ForEach(opsItemArray =>
        //                    {
        //                        var newTransTmpTableRow = new NewEditTrans_tmp();
        //                        var i = -1;

        //                        i += 1;
        //                        newTransTmpTableRow.TransID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.TransProcessCodeID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.ProcessOrd = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.StaticData = opsItemArray[i].Trim() != "" ? opsItemArray[i].ToLower() == "y" || opsItemArray[i].ToLower() == "1" : Convert.ToBoolean(null);

        //                        i += 1;
        //                        newTransTmpTableRow.DPTransID = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        i += 1;
        //                        newTransTmpTableRow.DPLotID = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        i += 1;
        //                        newTransTmpTableRow.AcctID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.SubAcctID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.ReliefMethod = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.SecID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.TCodeID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.DPTransCodeName = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        i += 1;
        //                        newTransTmpTableRow.EffectiveDate = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null);

        //                        i += 1;
        //                        newTransTmpTableRow.TradeDate = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null);

        //                        i += 1;
        //                        newTransTmpTableRow.SettleDate = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null);

        //                        i += 1;
        //                        newTransTmpTableRow.AcquisitionTradeDate = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null); // 

        //                        i += 1;
        //                        newTransTmpTableRow.AcquisitionSettleDate = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null); // 

        //                        i += 1;
        //                        newTransTmpTableRow.Shrs = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.OFShrs = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.Price = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.PriceMult = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.TradeCCYID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.SettleCCYID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.BaseCCYID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);


        //                        i += 1;
        //                        newTransTmpTableRow.TradeFXRate = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.SettleFXRate = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.BaseFXRate = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.GrossLocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.GrossBase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.Fees = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.ForeignWithhold = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.NetLocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.NetBase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.AILocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.AIBase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.AALocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.AABase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.OrigCostLocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.OrigCostBase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.BookCostLocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.BookCostBase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.RecordCostLocal = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.RecordCostBase = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.PurYld = opsItemArray[i].Trim() != "" ? Convert.ToDecimal(opsItemArray[i]) : Convert.ToDecimal(null);

        //                        i += 1;
        //                        newTransTmpTableRow.Comment = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        //i += 1;
        //                        //newTransTmpTableRow.CreatedDate = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null);

        //                        i += 1;
        //                        newTransTmpTableRow.Final = opsItemArray[i].Trim() != "" ? opsItemArray[i].ToLower() == "y" || opsItemArray[i].ToLower() == "1" : Convert.ToBoolean(null);

        //                        //i += 1;
        //                        //newTransTmpTableRow.LoadDateTime = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null)

        //                        i += 1;
        //                        newTransTmpTableRow.Stat = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        //i += 1;
        //                        //newTransTmpTableRow.RetiredDateTime = opsItemArray[i].Trim() != "" ? Convert.ToDateTime(opsItemArray[i]) : Convert.ToDateTime(null);

        //                        i += 1;
        //                        newTransTmpTableRow.LoadSourceID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.DPID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.OriginalTransID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.TransTypeID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.TransSubTypeID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.LinkAcctID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.LinkSubAcctID = opsItemArray[i].Trim() != "" ? Convert.ToInt32(opsItemArray[i]) : Convert.ToInt32(null);

        //                        i += 1;
        //                        newTransTmpTableRow.UserDefined1 = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        i += 1;
        //                        newTransTmpTableRow.UserDefined2 = opsItemArray[i].Trim() != "" ? Convert.ToString(opsItemArray[i]) : Convert.ToString(null);

        //                        newTransTmpTableRow.UIID = Convert.ToInt32(operations.IndexOf(opsItemArray) + 1);
        //                        newTransTmpTableRow.UserID = Convert.ToInt32(userId);

        //                        itemsToBeSaved.Add(newTransTmpTableRow);

        //                    });

        //                    _contextPWSRec.NewEditTrans_tmps.InsertAllOnSubmit(itemsToBeSaved);
        //                    _contextPWSRec.SubmitChanges();
        //                }

        //                if (gridColumnsID == "4") //New Account
        //                {

        //                    var allDataForUser = _contextPWS.New_Accounts_tmps.Where(f => f.UserID.Equals(userId)).AsQueryable();
        //                    _contextPWS.New_Accounts_tmps.DeleteAllOnSubmit(allDataForUser);

        //                    List<New_Accounts_tmp> itemsToBeSaved = new List<New_Accounts_tmp>();

        //                    operations.ForEach(opsItemArray =>
        //                    {
        //                        var newAccountTempTableRow = new New_Accounts_tmp();
        //                        newAccountTempTableRow.AcctCode = opsItemArray[0];
        //                        newAccountTempTableRow.AcctNum = opsItemArray[1];
        //                        newAccountTempTableRow.AcctName = opsItemArray[2];
        //                        newAccountTempTableRow.AcctNickNamePrimary = opsItemArray[3];
        //                        newAccountTempTableRow.BaseCurr = opsItemArray[4];
        //                        newAccountTempTableRow.LoadSourceID = opsItemArray[5].Trim() != "" ? Convert.ToInt32(opsItemArray[5]) : Convert.ToInt32(null);
        //                        newAccountTempTableRow.UserGroupID = opsItemArray[6].Trim() != "" ? Convert.ToInt32(opsItemArray[6]) : Convert.ToInt32(null);
        //                        newAccountTempTableRow.ReliefMethodID = opsItemArray[7].Trim() != "" ? Convert.ToInt32(opsItemArray[7]) : Convert.ToInt32(null);
        //                        newAccountTempTableRow.EntityCode = Convert.ToString(opsItemArray[8]);
        //                        newAccountTempTableRow.OwnedPct = opsItemArray[9].Trim() != "" ? Convert.ToDecimal(opsItemArray[9]) : Convert.ToDecimal(null);
        //                        newAccountTempTableRow.StartingDate = opsItemArray[10].Trim() != "" ? Convert.ToDateTime(opsItemArray[10]) : Convert.ToDateTime(null);
        //                        newAccountTempTableRow.EndingDate = opsItemArray[11].Trim() != "" ? Convert.ToDateTime(opsItemArray[11]) : Convert.ToDateTime(null);
        //                        newAccountTempTableRow.PortfolioCode = Convert.ToString(opsItemArray[12]);


        //                        newAccountTempTableRow.UserID = Convert.ToInt32(userId);
        //                        newAccountTempTableRow.UpdateDateTime = DateTime.Now;
        //                        newAccountTempTableRow.UIID = Convert.ToInt32(operations.IndexOf(opsItemArray) + 1);



        //                        itemsToBeSaved.Add(newAccountTempTableRow);
        //                    });

        //                    //save the records
        //                    _contextPWS.New_Accounts_tmps.InsertAllOnSubmit(itemsToBeSaved);
        //                    _contextPWS.SubmitChanges();
        //                }



        //                return string.Empty;
        //            }
        //            catch(Exception ex)
        //            {
        //                return "An error has occured";
        //            }

        //        }

        //        public string SaveTempGridCopy(List<List<ImportGridJson>> postData, string gridColumnsID, string userId)
        //        {

        //            //var refinedpostData = new List<List<ImportGridJson>>();

        //            //for(int i = 0; i < postData.Count; i++)
        //            //{
        //            //    postData[i] = postData[i].Where(f => !string.IsNullOrEmpty(f.Column)).ToList();
        //            //}

        //            var _context = new DataClasses1DataContext(connectionString);


        //            if (gridColumnsID == "4")
        //            {
        //                if(postData.Count > 0)
        //                {
        //                    var allDataForUser = _context.New_Accounts_tmps.Where(f => f.UserID.Equals(userId)).AsQueryable();
        //                    _context.New_Accounts_tmps.DeleteAllOnSubmit(allDataForUser);


        //                }
        //                List<New_Accounts_tmp> itemsToBeSaved = new List<New_Accounts_tmp>();

        //                foreach(var rowArray in postData)
        //                {
        //                    var newAccountTempTableRow = new New_Accounts_tmp();
        //                    foreach (var item in rowArray)
        //                    {

        //                        if (item.Required != "1" && string.IsNullOrEmpty(item.Value))
        //                        {
        //                            continue;
        //                        }


        //                        if (item.Column == "AcctCode")
        //                        {
        //                            newAccountTempTableRow.AcctCode = item.Value;
        //                        }
        //                        else if (item.Column == "AcctNum")
        //                        {
        //                            newAccountTempTableRow.AcctNum = item.Value;
        //                        }
        //                        else if (item.Column == "AcctName")
        //                        {
        //                            newAccountTempTableRow.AcctName = item.Value;
        //                        }
        //                        else if (item.Column == "AcctNickNamePrimary")
        //                        {
        //                            newAccountTempTableRow.AcctNickNamePrimary = item.Value;
        //                        }
        //                        else if (item.Column == "BaseCurr")
        //                        {
        //                            newAccountTempTableRow.BaseCurr = item.Value;
        //                        }
        //                        else if (item.Column == "LoadSourceID")
        //                        {
        //                            newAccountTempTableRow.LoadSourceID = Convert.ToInt32(item.Value);
        //                        }
        //                        else if (item.Column == "UserGroupID")
        //                        {
        //                            newAccountTempTableRow.UserGroupID = Convert.ToInt32(item.Value);
        //                        }
        //                        else if (item.Column == "ReliefMethodID")
        //                        {
        //                            newAccountTempTableRow.ReliefMethodID = Convert.ToInt32(item.Value);
        //                        }
        //                        else if (item.Column == "EntityCode")
        //                        {
        //                            newAccountTempTableRow.EntityCode = item.Value;
        //                        }
        //                        else if (item.Column == "OwnedPct")
        //                        {
        //                            newAccountTempTableRow.OwnedPct = !string.IsNullOrEmpty(item.Value) ? Convert.ToDecimal(item.Value) : Convert.ToDecimal(null);
        //                        }
        //                        else if (item.Column == "StartingDate")
        //                        {
        //                            newAccountTempTableRow.StartingDate = !string.IsNullOrEmpty(item.Value) ? Convert.ToDateTime(item.Value) : Convert.ToDateTime(null);
        //                        }
        //                        else if (item.Column == "EndingDate")
        //                        {
        //                            newAccountTempTableRow.EndingDate = !string.IsNullOrEmpty(item.Value) ? Convert.ToDateTime(item.Value) : Convert.ToDateTime(null);
        //                        }
        //                        else if (item.Column == "PortfolioCode")
        //                        {
        //                            newAccountTempTableRow.PortfolioCode = item.Value;
        //                        }

        //                        newAccountTempTableRow.UserID = Convert.ToInt32(userId);
        //                        newAccountTempTableRow.UpdateDateTime = DateTime.Now;
        //                        newAccountTempTableRow.UIID = Convert.ToInt32(item.RowIndex);
        //                    }

        //                    itemsToBeSaved.Add(newAccountTempTableRow);
        //                }

        //                //save the records
        //                _context.New_Accounts_tmps.InsertAllOnSubmit(itemsToBeSaved);
        //                _context.SubmitChanges();
        //            }

        //            return string.Empty;
        //        }


        //        private bool GetBoolValueFromNumner(string number)
        //        {
        //            return number == "1";
        //        }
    }
}