using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions.Json;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives.Zip;

namespace Jellyfin.Plugin.Bookshelf.Providers.ComicBookInfo
{
    /// <summary>
    /// Comic book info provider.
    /// </summary>
    public class ComicBookInfoProvider : IComicFileProvider
    {
        private readonly ILogger<ComicBookInfoProvider> _logger;

        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComicBookInfoProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">Instance of the <see cref="IFileSystem"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{ComicBookInfoProvider}"/> interface.</param>
        public ComicBookInfoProvider(IFileSystem fileSystem, ILogger<ComicBookInfoProvider> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask<MetadataResult<Book>> ReadMetadata(ItemInfo info, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            var path = GetComicBookFile(info.Path)?.FullName;

            if (path is null)
            {
                _logger.LogError("Could not load Comic for {Path}", info.Path);
                return new MetadataResult<Book> { HasMetadata = false };
            }

            try
            {
                await using Stream stream = File.OpenRead(path);

                // not yet async: https://github.com/adamhathcock/sharpcompress/pull/565
                using var archive = ZipArchive.Open(stream);

                if (archive.IsComplete)
                {
                    var volume = archive.Volumes.First();
                    if (volume.Comment is not null)
                    {
                        await using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(volume.Comment));
                        var comicBookMetadata = await JsonSerializer.DeserializeAsync<ComicBookInfoFormat>(jsonStream, JsonDefaults.Options, cancellationToken);

                        if (comicBookMetadata is null)
                        {
                            _logger.LogError("Failed to load ComicBookInfo metadata from archive comment for {Path}", info.Path);
                            return new MetadataResult<Book> { HasMetadata = false };
                        }

                        return SaveMetadata(comicBookMetadata);
                    }
                    else
                    {
                        _logger.LogInformation("{Path} does not contain any ComicBookInfo metadata", info.Path);
                        return new MetadataResult<Book> { HasMetadata = false };
                    }
                }

                _logger.LogError("Could not load ComicBookInfo metadata for {Path}", info.Path);
                return new MetadataResult<Book> { HasMetadata = false };
            }
            catch (Exception)
            {
                _logger.LogError("Failed to load ComicBookInfo metadata for {Path}", info.Path);
                return new MetadataResult<Book> { HasMetadata = false };
            }
        }

        /// <inheritdoc />
        public bool HasItemChanged(BaseItem item)
        {
            var file = GetComicBookFile(item.Path);

            if (file is null)
            {
                return false;
            }

            return file.Exists && _fileSystem.GetLastWriteTimeUtc(file) > item.DateLastSaved;
        }

        private MetadataResult<Book> SaveMetadata(ComicBookInfoFormat comic)
        {
            if (comic.Metadata is null)
            {
                return new MetadataResult<Book> { HasMetadata = false };
            }

            var book = ReadComicBookMetadata(comic.Metadata);

            if (book is null)
            {
                return new MetadataResult<Book> { HasMetadata = false };
            }

            var metadataResult = new MetadataResult<Book> { Item = book, HasMetadata = true };

            if (comic.Metadata.Language is not null)
            {
                metadataResult.ResultLanguage = ReadCultureInfoAsThreeLetterIsoInto(comic.Metadata.Language);
            }

            if (comic.Metadata.Credits.Length > 0)
            {
                foreach (var person in comic.Metadata.Credits)
                {
                    if (person.Person is null || person.Role is null)
                    {
                        continue;
                    }

                    var personInfo = new PersonInfo { Name = person.Person, Type = person.Role };
                    metadataResult.AddPerson(personInfo);
                }
            }

            return metadataResult;
        }

        private Book? ReadComicBookMetadata(ComicBookInfoMetadata comic)
        {
            var book = new Book();
            var hasFoundMetadata = false;

            hasFoundMetadata |= ReadStringInto(comic.Title, title => book.Name = title);
            hasFoundMetadata |= ReadStringInto(comic.Series, series => book.SeriesName = series);
            hasFoundMetadata |= ReadStringInto(comic.Genre, genre => book.AddGenre(genre));
            hasFoundMetadata |= ReadStringInto(comic.Comments, overview => book.Overview = overview);
            hasFoundMetadata |= ReadStringInto(comic.Publisher, publisher => book.SetStudios(new[] { publisher }));

            if (comic.PublicationYear is not null)
            {
                book.ProductionYear = comic.PublicationYear;
                hasFoundMetadata |= true;
            }

            if (comic.Issue is not null)
            {
                book.IndexNumber = comic.Issue;
                hasFoundMetadata |= true;
            }

            if (comic.Tags is not null && comic.Tags.Length > 0)
            {
                book.Tags = comic.Tags;
                hasFoundMetadata |= true;
            }

            if (comic.PublicationYear is not null && comic.PublicationMonth is not null)
            {
                book.PremiereDate = ReadTwoPartDateInto(comic.PublicationYear.Value, comic.PublicationMonth.Value);
                hasFoundMetadata |= true;
            }

            if (hasFoundMetadata)
            {
                return book;
            }
            else
            {
                return null;
            }
        }

        private bool ReadStringInto(string? data, Action<string> commitResult)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                commitResult(data);
                return true;
            }

            return false;
        }

        private DateTime? ReadTwoPartDateInto(int year, int month)
        {
            // Try-Catch because DateTime actually wants a real date, how boring
            try
            {
                // The format does not provide a day, set it to be always the first day of the month
                var dateTime = new DateTime(year, month, 1);
                return dateTime;
            }
            catch (Exception)
            {
                // Nothing to do here
                return null;
            }
        }

        private string? ReadCultureInfoAsThreeLetterIsoInto(string language)
        {
            try
            {
                return new CultureInfo(language).ThreeLetterISOLanguageName;
            }
            catch (Exception)
            {
                // Ignored
                return null;
            }
        }

        private FileSystemMetadata? GetComicBookFile(string path)
        {
            var fileInfo = _fileSystem.GetFileSystemInfo(path);

            if (fileInfo.IsDirectory)
            {
                return null;
            }

            // Only parse files that are known to have internal metadata
            return fileInfo.Extension.Equals(".cbz", StringComparison.OrdinalIgnoreCase) ? fileInfo : null;
        }
    }
}
