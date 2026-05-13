using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// Rezultatul unui singur handler din lant.
     /// Colectat in ApprovalResult.Checks pentru trasabilitate completa.
     /// </summary>
     public class ApprovalCheck
     {
          public string         HandlerName { get; set; } = "";
          public ApprovalStatus Status      { get; set; }
          public string         Message     { get; set; } = "";
          public DateTime       CheckedAt   { get; set; } = DateTime.UtcNow;

          // Label util in raspunsul API
          public string StatusLabel => Status switch
          {
               ApprovalStatus.Approved              => "✓ Aprobat",
               ApprovalStatus.PendingManagerApproval => "⚠ Necesita aprobare",
               ApprovalStatus.Rejected              => "✗ Respins",
               _                                    => Status.ToString()
          };
     }
}
