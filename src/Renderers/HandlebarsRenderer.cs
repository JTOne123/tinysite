﻿using System;
using System.Collections.Generic;
using FuManchu;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("hb")]
    [Render("handlebar")]
    [Render("handlebars")]
    public class HandlebarsRenderer : IRenderer
    {
        private object _renderLock = new object();

        private Dictionary<string, Func<object, string>> compiledTemplates = new Dictionary<string, Func<object, string>>();

        public string Render(SourceFile sourceFile, string template, object data)
        {
            var path = sourceFile.SourcePath;

            lock (_renderLock)
            {
                try
                {
                    Func<object, string> compiledTemplate;

                    if (!this.compiledTemplates.TryGetValue(path, out compiledTemplate))
                    {
                        compiledTemplate = Handlebars.Compile(path, template);

                        this.compiledTemplates.Add(path, compiledTemplate);
                    }

                    var result = compiledTemplate(data);

                    return result;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Handelbars failure while processing: {0}, error: {1}", path, e.Message);
                }

                return null;
            }
        }

        public void Unload(IEnumerable<string> paths)
        {
            lock (_renderLock)
            {
                foreach (var path in paths)
                {
                    this.compiledTemplates.Remove(path);
                }
            }
        }
    }
}
