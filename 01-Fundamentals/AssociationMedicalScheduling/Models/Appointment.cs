namespace AssociationMedicalScheduling.Models;

public sealed class Appointment
{
    public Appointment(Patient patient, Doctor doctor, DateTime startsAt, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(doctor);

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        Id = Guid.NewGuid();
        Patient = patient;
        Doctor = doctor;
        StartsAt = startsAt;
        Duration = duration;
    }

    public Guid Id { get; }
    public Patient Patient { get; }
    public Doctor Doctor { get; }
    public DateTime StartsAt { get; }
    public TimeSpan Duration { get; }
    public bool IsCancelled { get; private set; }
    public DateTime EndsAt => StartsAt.Add(Duration);

    public bool Overlaps(DateTime otherStartsAt, TimeSpan otherDuration)
    {
        DateTime otherEndsAt = otherStartsAt.Add(otherDuration);
        return StartsAt < otherEndsAt && otherStartsAt < EndsAt;
    }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public override string ToString()
    {
        string status = IsCancelled ? "CANCELADA" : "ATIVA";
        return $"[{status}] {StartsAt:HH:mm} - {EndsAt:HH:mm} | {Patient.FullName} com {Doctor.FullName}";
    }
}