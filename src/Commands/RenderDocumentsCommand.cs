﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TinySite.Extensions;
using TinySite.Models;
using TinySite.Services;

namespace TinySite.Commands
{
    public class RenderDocumentsCommand
    {
        public IDictionary<string, RenderingEngine> Engines { private get; set; }

        public Site Site { private get; set; }

        public int RenderedDocuments { get; private set; }

        public int Execute()
        {
            using (var tx = new RenderingTransaction(this.Engines, this.Site))
            {
                IEnumerable<DocumentFile> renderedDocuments;
                using (var capture = Statistics.Current.Start(StatisticTiming.RenderDocumentContent))
                {
                    renderedDocuments = this.Site.Documents
                                        .Where(d => !d.Draft)
                                        .AsParallel()
                                        .Select(this.RenderDocument)
                                        .ToList();
                }

                using (var capture = Statistics.Current.Start(StatisticTiming.RenderLayouts))
                {
                    foreach (var document in renderedDocuments)
                    {
                        var layoutName = document.Metadata.Get<string>("layout", "default");

                        var layout = this.Site.Layouts[layoutName];

                        document.RenderedContent = this.RenderDocumentContentUsingLayout(document, document.Content, layout);

                        document.Rendered = true;
                    }
                }

                return this.RenderedDocuments = renderedDocuments.Count();
            }
        }

        private DocumentFile RenderDocument(DocumentFile document)
        {
            var content = document.SourceContent;

            var layoutName = document.Metadata.Get<string>("layout", "default");

            var layout = this.Site.Layouts[layoutName];

            foreach (var extension in document.ExtensionsForRendering)
            {
                content = this.RenderContentForExtension(document.SourcePath, content, extension, document, content, layout);
            }

            document.Content = content;

            if (String.IsNullOrEmpty(document.Summary) && !String.IsNullOrEmpty(document.Content))
            {
                document.Summary = Summarize(document.Content);
            }

            return document;
        }

        private string RenderContentForExtension(string path, string content, string extension, DocumentFile document, string documentContent, LayoutFile layout)
        {
            var engine = this.Engines[extension];

            var book = document.Book;

            var paginator = document.Paginator;

            var data = new CaseInsensitiveExpando();
            data.Add("Site", this.Site.GetAsDynamic());
            data.Add("Document", document.GetAsDynamic(documentContent));
            data.Add("Layout", layout == null ? null : layout.GetAsDynamic());
            data.Add("Book", book == null ? null : book.GetAsDynamic(document));
            data.Add("Paginator", paginator == null ? null : paginator.GetAsDynamic());
            data.Add("Books", this.Site.Books == null ? null : this.Site.Books.Select(b => b.GetAsDynamic(document)));

            return engine.Render(path, content, data);
        }

        private string RenderDocumentContentUsingLayout(DocumentFile document, string documentContent, LayoutFile layout)
        {
            var content = this.RenderContentForExtension(layout.SourcePath, layout.SourceContent, layout.Extension, document, documentContent, layout);

            string parentLayoutName;

            if (layout.Metadata.TryGet<string>("layout", out parentLayoutName))
            {
                var parentLayout = this.Site.Layouts[parentLayoutName];

                content = this.RenderDocumentContentUsingLayout(document, content, parentLayout);
            }

            return content;
        }

        private static string Summarize(string content)
        {
            string summary = null;

            Match match = Regex.Match(content, "<p>.*?</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success && match.Value != content)
            {
                summary = match.Value;
            }

            return summary;
        }
    }
}
