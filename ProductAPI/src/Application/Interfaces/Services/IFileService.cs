using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IFileService
    {
        Task StoreFilesInDatabaseAsync(List<AttachedFileUpsertDto> files);
        Task<List<AttachedFileUpsertDto>> StoreFilesLocallyAsync(Guid entityId, List<AttachedFileDto> files);
        /// <summary>
        /// Stores files locally and then in database.
        /// </summary>
        Task StoreFilesAsync(Guid entityId, List<AttachedFileDto> files);
    }
}
