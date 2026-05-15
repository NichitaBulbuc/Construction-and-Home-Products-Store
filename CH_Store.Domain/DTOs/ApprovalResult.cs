using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// Rezultatul agregat al intregului lant de aprobare.
     /// Contine statusul final si detaliile fiecarui handler.
     /// </summary>
     public class ApprovalResult
     {
          public ApprovalStatus    Status          { get; set; } = ApprovalStatus.Approved;
          public string?           RejectionReason { get; set; }
          public List<string>      PendingReasons  { get; set; } = new();
          public List<ApprovalCheck> Checks        { get; set; } = new();
          public DateTime          ProcessedAt     { get; set; } = DateTime.UtcNow;

          // ── Proprietati derivate ───────────────────────────────────────────
          public bool IsApproved   => Status == ApprovalStatus.Approved;
          public bool IsRejected   => Status == ApprovalStatus.Rejected;
          public bool NeedsReview  => Status == ApprovalStatus.PendingManagerApproval;

          public string OrderStatusToSet => Status switch
          {
               ApprovalStatus.Approved              => "New",
               ApprovalStatus.PendingManagerApproval => "PendingApproval",
               _                                    => "Rejected"
          };

          // ── Factory helpers ───────────────────────────────────────────────
          public static ApprovalResult Approve(List<ApprovalCheck> checks) => new()
          {
               Status = ApprovalStatus.Approved,
               Checks = checks
          };

          public static ApprovalResult Reject(List<ApprovalCheck> checks, string reason) => new()
          {
               Status          = ApprovalStatus.Rejected,
               RejectionReason = reason,
               Checks          = checks
          };

          public static ApprovalResult Pending(List<ApprovalCheck> checks, List<string> reasons) => new()
          {
               Status         = ApprovalStatus.PendingManagerApproval,
               PendingReasons = reasons,
               Checks         = checks
          };
     }
}
