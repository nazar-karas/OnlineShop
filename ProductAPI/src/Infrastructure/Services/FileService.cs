using Application.DTO;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }
        public async Task StoreFilesAsync(Guid entityId, List<AttachedFileDto> files)
        {
            var localFiles = await StoreFilesLocallyAsync(entityId, files);
            localFiles.ForEach(f => f.EntityId = entityId);
            await StoreFilesInDatabaseAsync(localFiles);
        }

        public async Task StoreFilesInDatabaseAsync(List<AttachedFileUpsertDto> files)
        {
            await _fileRepository.StoreFilesInDatabaseAsync(files);
        }

        public async Task<List<AttachedFileUpsertDto>> StoreFilesLocallyAsync(Guid entityId, List<AttachedFileDto> files)
        {
            var storedFiles = await _fileRepository.StoreFilesLocallyAsync(entityId, files);
            return storedFiles;
        }
    }
}
