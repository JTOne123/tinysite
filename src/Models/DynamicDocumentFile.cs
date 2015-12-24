﻿using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class DynamicDocumentFile : DynamicOutputFile
    {
        public DynamicDocumentFile(DocumentFile activeDocument, DocumentFile document)
            : base(document)
        {
            this.ActiveDocument = activeDocument;
            this.Document = document;
        }

        private DocumentFile ActiveDocument { get; }

        private DocumentFile Document { get; }

        public DocumentFile GetDocument()
        {
            return this.Document;
        }

        protected override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            data.Add(nameof(this.Document.Author), this.Document.Author);
            data.Add(nameof(this.Document.Layout), this.Document.Layout);
            data.Add(nameof(this.Document.Content), this.Document.Content);
            data.Add(nameof(this.Document.Draft), this.Document.Draft);
            data.Add(nameof(this.Document.Id), this.Document.Id);
            data.Add(nameof(this.Document.Order), this.Document.Order);
            data.Add(nameof(this.Document.PaginateQuery), this.Document.PaginateQuery);
            data.Add(nameof(this.Document.ParentId), this.Document.ParentId);
            data.Add(nameof(this.Document.SourceContent), this.Document.SourceContent);
            data.Add(nameof(this.Document.Summary), this.Document.Summary);
            data.Add(nameof(this.Document.NextDocument), new Lazy<object>(GetNextDocument));
            data.Add(nameof(this.Document.ParentDocument), new Lazy<object>(GetParentDocument));
            data.Add(nameof(this.Document.PreviousDocument), new Lazy<object>(GetPreviousDocument));
            data.Add(nameof(this.Document.Book), new Lazy<object>(GetBook));
            data.Add(nameof(this.Document.Chapter), new Lazy<object>(GetChapter));
            data.Add(nameof(this.Document.Paginator), new Lazy<object>(GetPaginator));

            this.Document.Metadata?.AssignTo(this.Document.SourceRelativePath, data);

            return data;
        }

        protected override bool TrySetValue(string key, object value)
        {
            if (base.TrySetValue(key, value))
            {
                this.Document.Metadata.Add(key, value);
                return true;
            }

            return false;
        }

        private object GetNextDocument()
        {
            if (this.Document.NextDocument != null)
            {
                this.ActiveDocument.AddContributingFile(this.Document.NextDocument);
                return new DynamicDocumentFile(this.ActiveDocument, this.Document.NextDocument);
            }

            return null;
        }

        private object GetParentDocument()
        {
            if (this.Document.ParentDocument != null)
            {
                this.ActiveDocument.AddContributingFile(this.Document.ParentDocument);
                return new DynamicDocumentFile(this.ActiveDocument, this.Document.ParentDocument);
            }

            return null;
        }

        private object GetPreviousDocument()
        {
            if (this.Document.PreviousDocument != null)
            {
                this.ActiveDocument.AddContributingFile(this.Document.PreviousDocument);
                return new DynamicDocumentFile(this.ActiveDocument, this.Document.PreviousDocument);
            }

            return null;
        }

        private object GetBook()
        {
            if (this.Document.Book != null)
            {
                return new DynamicBook(this.ActiveDocument, this.Document.Book);
            }

            return null;
        }

        private object GetChapter()
        {
            if (this.Document.Chapter != null)
            {
                this.ActiveDocument.AddContributingFile(this.Document.Chapter.Document);
                return new DynamicBookPage(this.ActiveDocument, this.Document.Chapter);
            }

            return null;
        }

        private object GetPaginator()
        {
            if (this.Document.Paginator != null)
            {
                return new DynamicPaginator(this.ActiveDocument, this.Document.Paginator);
            }

            return null;
        }
    }
}