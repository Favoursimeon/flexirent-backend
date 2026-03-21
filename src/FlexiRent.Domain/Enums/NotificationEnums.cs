namespace FlexiRent.Domain.Enums;

public enum NotificationType
{
    BookingConfirmed,
    BookingRejected,
    BookingCancelled,
    PaymentDue,
    PaymentReceived,
    PaymentFailed,
    PaymentOverdue,
    PropertyApproved,
    PropertyRejected,
    PropertyMatch,
    LeaseExpiringSoon,
    NewMessage,
    VerificationApproved,
    VerificationRejected,
    General
}