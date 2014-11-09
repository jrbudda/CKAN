using NUnit.Framework;
using System;
using System.IO;

namespace CKANTests
{
    [TestFixture()]
    public class Cache
    {
        private readonly string cache_dir = Path.Combine(Tests.TestData.DataDir(),"cache_test");

        private CKAN.NetFileCache cache;

        [SetUp()]
        public void MakeCache()
        {
            Directory.CreateDirectory(cache_dir);
            cache = new CKAN.NetFileCache(cache_dir);
        }

        [TearDown()]
        public void RemoveCache()
        {
            Directory.Delete(cache_dir, true);
        }

        [Test]
        public void Sanity()
        {
            Assert.IsInstanceOf<CKAN.NetFileCache>(cache);
            Assert.IsTrue(Directory.Exists(cache.GetCachePath()));
        }

        [Test]
        public void StoreRetrieve()
        {
            Uri url = new Uri("http://example.com/");
            string file = Tests.TestData.DogeCoinFlagZip();

            // Sanity check, our cache dir is there, right?
            Assert.IsTrue(Directory.Exists(cache.GetCachePath()));

            // Our URL shouldn't be cached to begin with.
            Assert.IsFalse(cache.IsCached(url));

            // Store our file.
            cache.Store(url, file);

            // Now it should be cached.
            Assert.IsTrue(cache.IsCached(url));

            // Check contents match.
            string cached_file = cache.GetCachedFilename(url);
            FileAssert.AreEqual(file, cached_file);
        }

        [Test]
        public void NamingHints()
        {
            Uri url = new Uri("http://example.com/");
            string file = Tests.TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));
            cache.Store(url, file, "cheesy.zip");

            StringAssert.EndsWith("cheesy.zip", cache.GetCachedFilename(url));
        }

        [Test]
        public void StoreRemove()
        {
            Uri url = new Uri("http://example.com/");
            string file = Tests.TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));
            cache.Store(url, file);
            Assert.IsTrue(cache.IsCached(url));

            cache.Remove(url);

            Assert.IsFalse(cache.IsCached(url));
        }

        [Test()]
        public void CacheKraken()
        {
            string dir = "/this/path/better/not/exist";

            try
            {
                new CKAN.NetFileCache(dir);
            }
            catch (CKAN.DirectoryNotFoundKraken kraken)
            {
                Assert.AreSame(dir,kraken.directory);
            }
        }

        [Test]
        public void DoubleCache()
        {
            // Store and flip files in our cache. We should always get
            // the most recent file we store for any given URL.

            Uri url = new Uri("http://Double.Rainbow.What.Does.It.Mean/");
            Assert.IsFalse(cache.IsCached(url));

            string file1 = Tests.TestData.DogeCoinFlagZip();
            string file2 = Tests.TestData.ModuleManagerZip();

            cache.Store(url, file1);
            FileAssert.AreEqual(file1, cache.GetCachedFilename(url));

            cache.Store(url, file2);
            FileAssert.AreEqual(file2, cache.GetCachedFilename(url));

            cache.Store(url, file1);
            FileAssert.AreEqual(file1, cache.GetCachedFilename(url));
        }
    }
}
