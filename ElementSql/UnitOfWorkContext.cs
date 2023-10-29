using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    internal class UnitOfWorkContext : IUnitOfWorkContext
    {
        public bool WasSuccessful { get; set; }
        public IDbTransaction Transaction { get; }

        public UnitOfWorkContext(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Transaction = unitOfWork.GetTransaction();
        }

        public void Dispose()
        {
            if (WasSuccessful)
                _unitOfWork.Commit();
            else
                _unitOfWork.RollBack();
        }

        private readonly IUnitOfWork _unitOfWork;
    }
}
