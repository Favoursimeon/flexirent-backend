namespace FlexiRent.Domain.Enums;

public enum PaymentStatus
{
    Pending,
    Processing,
    Successful,
    Failed,
    Refunded
}

public enum LeaseStatus
{
    Active,
    Expired,
    Terminated,
    Renewed
}

public enum PaymentScheduleStatus
{
    Upcoming,
    Due,
    Paid,
    Overdue,
    Waived
}

public enum PaymentAccountType
{
    BankAccount,
    MobileMoney,
    Card
}
