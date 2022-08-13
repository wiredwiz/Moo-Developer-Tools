namespace Org.Edgerunner.ANTLR4.Tools.Common;

/// <summary>Represents a document</summary>
public class Document
{
   /// <summary>
   /// Initializes a new instance of the <see cref="Document"/> class.
   /// </summary>
   /// <param name="id">The identifier.</param>
   /// <param name="name">The name.</param>
   public Document(string id, string name)
   {
      Id = id;
      Name = name;
   }

   /// <summary>Gets or sets the document's unique identifier.</summary>
   /// <value>The unique identifier.</value>
   public string Id { get; set; }

   /// <summary>Gets or sets the document's name.</summary>
   /// <value>The document's name.</value>
   public string Name { get; set; }
}