using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IFileRepository
    {
        Task StoreFilesInDatabaseAsync(List<AttachedFileUpsertDto> files);
        Task<List<AttachedFileUpsertDto>> StoreFilesLocallyAsync(Guid entityId, List<AttachedFileDto> files);
    }
}
