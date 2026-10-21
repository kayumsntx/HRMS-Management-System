namespace HRMS.Domain.Enums;

public enum RequisitionStatus { Pending, Approved, Rejected, Cancelled }
public enum ChangeRequestStatus { Pending, Approved, Rejected, Cancelled }
public enum CareerActionType { Promotion, Transfer, DepartmentChange, DesignationChange, GradeChange }
public enum SeparationType { Resignation, Termination, Retirement, Other }
public enum SeparationStatus { Pending, Approved, Rejected, Completed, Cancelled }
public enum SettlementStatus { Draft, Approved, Paid }
public enum ManpowerStatus { Pending, Approved, Rejected, Closed }
public enum VacancyStatus { Open, Closed, Cancelled }
public enum ApplicantStatus { Applied, Shortlisted, Interviewed, Selected, Rejected, Hired }
public enum InterviewStatus { Scheduled, Completed, Cancelled }
public enum InterviewResult { Pass, Fail, Hold }
public enum OfferStatus { Sent, Accepted, Rejected, Expired }
public enum GoalStatus { Open, InProgress, Completed, Cancelled }
public enum EvaluationStatus { Draft, Submitted, Approved }
public enum TrainingStatus { Planned, Running, Completed, Cancelled }
public enum ParticipantStatus { Enrolled, Completed, Dropped }
