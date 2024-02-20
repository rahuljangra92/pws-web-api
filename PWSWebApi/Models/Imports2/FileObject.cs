using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.Imports2
{
    public class FileObject
    {
        public string FileName { get; set; }
        public string FileString { get; set; } = string.Empty;
        public byte[] FileAsBytes => string.IsNullOrEmpty(FileString) ? null : Convert.FromBase64String(FileString);
        //public ArrayList TransactionRows { get; set; } = new ArrayList();
    }
}