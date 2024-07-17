using Application.DTO;
using Application.Interfaces.Repositories;
using AutoMapper;
using Azure.Core;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly ApplicationDbContext _context;
        private IMapper _mapper;

        public FileRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task StoreFilesInDatabaseAsync(List<AttachedFileUpsertDto> files)
        {
            var filesFromDb = await _context.AttachedFiles.ToListAsync();

            foreach (var file in files)
            {
                var existingFile = filesFromDb.FirstOrDefault(x => x.Id == file.Id);

                if (existingFile == null)
                {
                    var fileToAdd = new AttachedFile();
                    fileToAdd.Id = Guid.NewGuid();
                    fileToAdd.FileName = file.FileName;
                    fileToAdd.Location = file.Location;
                    fileToAdd.Checksum = file.Checksum;
                    fileToAdd.ProductId = file.EntityId;

                    await _context.AddAsync(fileToAdd);
                }
                else
                {
                    existingFile.FileName = file.FileName;
                    existingFile.Location = file.Location;
                    existingFile.Checksum = file.Checksum;
                    existingFile.ProductId = file.EntityId;

                    _context.Update(existingFile);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<AttachedFileUpsertDto>> StoreFilesLocallyAsync(Guid entityId, List<AttachedFileDto> files)
        {
            var filesToStoreInDb = new List<AttachedFileUpsertDto>();

            foreach (var file in files)
            {
                byte[] contents = Convert.FromBase64String(file.FileContents);
                string path = Directory.GetParent(Environment.CurrentDirectory) + @$"\Files\Products\{entityId}";

                if (!Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                path = path + @$"\{file.FileName}";

                using (Stream strem = new FileStream(path, FileMode.Create))
                {
                    await strem.WriteAsync(contents);
                }

                //System.IO.File.WriteAllBytes(path, contents);

                var fileToStore = new AttachedFileUpsertDto()
                {
                    FileName = file.FileName,
                    Location = path,
                    Checksum = DataHelper.ComputeFileChecksum(contents),
                    EntityId = entityId
                };

                filesToStoreInDb.Add(fileToStore);
            }

            return filesToStoreInDb;
        }
    }
}
