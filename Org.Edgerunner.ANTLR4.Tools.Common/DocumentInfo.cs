namespace Org.Edgerunner.ANTLR4.Tools.Common;

/// <summary>Represents a document</summary>
public class DocumentInfo
{
   /// <summary>
   /// Initializes a new instance of the <see cref="DocumentInfo" /> class.
   /// </summary>
   /// <param name="id">The identifier.</param>
   /// <param name="path">The path.</param>
   /// <param name="name">The name.</param>
   public DocumentInfo(string id, string path, string name)
   {
      Id = id;
      Path = path;
      Name = name;
   }

   /// <summary>Gets or sets the document's unique identifier.</summary>
   /// <value>The unique identifier.</value>
   public string Id { get; set; }

   /// <summary>
   /// Gets or sets the document path.
   /// </summary>
   /// <value>
   /// The document path.
   /// </value>
   public string Path { get; set; }

   /// <summary>Gets or sets the document's name.</summary>
   /// <value>The document's name.</value>
   /// <remarks>The name is used for display-friendly purposes.</remarks>
   public string Name { get; set; }

    /// <summary>
    /// Gets or sets the document tag.
    /// </summary>
    /// <value>The document tag.</value>
    /// <remarks>This is a data tag for storing miscellaneous data.</remarks>
    public string Tag { get; set; }
}