using Application.DTO;
using Azure.Core;
using Domain.Common;
using Domain.Entities;
using System.Security.Cryptography;

namespace Infrastructure.Helpers
{
    public class DataHelper
    {
        public static PagedCollection<T> ToPagedCollection<T>(IEnumerable<T> items, int pageNumber, int pageSize) where T : class
        {
            PagedCollection<T> pagedItems = new PagedCollection<T>();
            pagedItems.Items = items.Skip((pageSize * pageNumber) - pageSize).Take(pageSize);
            pagedItems.PageNumber = pageNumber;
            pagedItems.PageSize = pageSize;
            pagedItems.TotalItems = items.Count();
            double totalPages = Math.Ceiling((double)items.Count() / pageSize);
            pagedItems.TotalPages = (int)totalPages;
            return pagedItems;
        }
        
        public static string ComputeFileChecksum(byte[] contents)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashInBytes = sha256.ComputeHash(contents);
                string hash = BitConverter.ToString(hashInBytes).Replace("-", "");
                return hash;
            }
        }

        /// <returns>Path to the created file.</returns>
        public static string CreateFilesLocally(Guid productId, string filename, string fileContents)
        {
            byte[] contents = Convert.FromBase64String(fileContents);
            string path = Directory.GetParent(Environment.CurrentDirectory) + @$"\Files\Products\{productId}";

            if (!Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            path = path + @$"\{filename}";

            using (Stream strem = new FileStream(path, FileMode.Create))
            {
                strem.Write(contents);
            }

            return path;
        }
    }
}
