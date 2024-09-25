using GCGInterviews.Interfaces;
using GCGInterviews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews
{
    public class HawkSearchLogHelper
    {
        private ZnodeImportProcessLog znodeImportProcessLog;
        private ZnodeImportProcessLog processLog;
        private IZnodeRepository<ZnodeImportProcessLog> _importProcessLogRepository;
        private IZnodeRepository<ZnodeImportLog> _importLogRepository;
        private int lineNumber;
        private bool IsActive;

        public HawkSearchLogHelper(bool isActive = false)
        {
            _importProcessLogRepository = new ZnodeRepository<ZnodeImportProcessLog>();
            _importLogRepository = new ZnodeRepository<ZnodeImportLog>();
            lineNumber = 1;
            IsActive = isActive;
        }

        public ZnodeImportProcessLog StartImportLog(int scheduleId)
        {
            if (!IsActive)
            {
                return null;
            }
            znodeImportProcessLog = new ZnodeImportProcessLog();
            znodeImportProcessLog.ImportTemplateId = 30;
            znodeImportProcessLog.Status = "Started";
            znodeImportProcessLog.ProcessStartedDate = DateTime.Now;
            znodeImportProcessLog.ProcessCompletedDate = DateTime.Now;
            znodeImportProcessLog.CreatedBy = 0;
            znodeImportProcessLog.CreatedDate = DateTime.Now;
            znodeImportProcessLog.ModifiedBy = 0;
            znodeImportProcessLog.ModifiedDate = DateTime.Now;
            znodeImportProcessLog.ERPTaskSchedulerId = scheduleId;

            processLog = _importProcessLogRepository.Insert(znodeImportProcessLog);
            return processLog;
        }

        public void UpdateImportLog(string importName, int errorCode, string data)
        {
            if (processLog == null) return;
            ZnodeImportLog znodeImportLog = new ZnodeImportLog();
            znodeImportLog.ImportProcessLogId = znodeImportProcessLog.ImportProcessLogId;
            znodeImportLog.ErrorDescription = errorCode.ToString();
            znodeImportLog.ColumnName = importName;
            znodeImportLog.Data = data;
            znodeImportLog.RowNumber = lineNumber;
            znodeImportLog.CreatedBy = 0;
            znodeImportLog.CreatedDate = DateTime.Now;
            znodeImportLog.ModifiedBy = 0;
            znodeImportLog.ModifiedDate = DateTime.Now;
            ZnodeImportLog log = _importLogRepository.Insert(znodeImportLog);

            lineNumber += 1;
        }

        public void FinishImportLog(bool isSuccess)
        {
            if (processLog == null) return;
            znodeImportProcessLog.Status = (isSuccess ? "Success" : "Failed");
            znodeImportProcessLog.ProcessCompletedDate = DateTime.Now;
            znodeImportProcessLog.ModifiedDate = DateTime.Now;
            znodeImportProcessLog.SuccessRecordCount = (isSuccess ? 1 : 0);
            znodeImportProcessLog.FailedRecordcount = (!isSuccess ? 1 : 0);
            znodeImportProcessLog.TotalProcessedRecords = 1;

            bool success = _importProcessLogRepository.Update(znodeImportProcessLog);
        }
        public void FinishImportLog(bool isSuccess, long total = 0, long suceedCount = 0, long failedCount = 0)
        {
            if (processLog == null) return;
            znodeImportProcessLog.Status = (isSuccess ? "Success" : "Failed");
            znodeImportProcessLog.ProcessCompletedDate = DateTime.Now;
            znodeImportProcessLog.ModifiedDate = DateTime.Now;
            znodeImportProcessLog.SuccessRecordCount = (isSuccess ? suceedCount : 0);
            znodeImportProcessLog.FailedRecordcount = (!isSuccess ? failedCount : 0);
            znodeImportProcessLog.TotalProcessedRecords = total;

            bool success = _importProcessLogRepository.Update(znodeImportProcessLog);
        }

        public void DeleteLogs()
        {
            _importLogRepository.Delete("1 = 1");
            _importProcessLogRepository.Delete("1 = 1");
        }
    }
}
