﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
namespace TiltBrush
{
    public class FilesystemSketchCollection : IResource, IResourceCollection
    {
        private string m_Path;
        private DirectoryInfo m_Dir;

        public FilesystemSketchCollection(string path, string name)
        {
            m_Path = path;
            Name = name;
        }

        public string Name { get; private set; }

        public Uri Uri { get; private set; }

        public Texture2D PreviewImage { get; }

        public string Description { get; }

        public Author[] Authors { get; set; }

        public ResourceLicense License { get; }

#pragma warning disable 1998
        public async Task<bool> LoadPreviewAsync()
        {
            // TODO: Perhaps to something clever with having a thumbnail in a .meta subdir?
            return false; // false means no preview created.
        }

        public async Task InitAsync()
        {
            m_Dir = new DirectoryInfo(m_Path);
            if (Name == null)
            {
                Name = m_Dir.Name;
            }
            Uri = new Uri($"file://{m_Dir.FullName}");
        }

        public async IAsyncEnumerable<IResource> Contents()
        {
            foreach (var dirInfo in m_Dir.EnumerateDirectories())
            {
                if (dirInfo.Name.StartsWith("."))
                {
                    continue;
                }
                yield return new FilesystemSketchCollection(dirInfo.FullName, dirInfo.Name);
            }

            foreach (var fileInfo in m_Dir.EnumerateFiles("*.tilt"))
            {
                yield return new FilesystemSketch(fileInfo.FullName);
            }
        }

        public async Task<Stream> GetStreamAsync()
        {
            throw new NotImplementedException();
        }
#pragma warning restore 1998

    }
}
