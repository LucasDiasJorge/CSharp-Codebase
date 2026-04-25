using AssociationMedicalScheduling.Models;

namespace AssociationMedicalScheduling.Services;

public sealed class AppointmentBook
{
    private readonly List<Appointment> appointments;

    public AppointmentBook()
    {
        appointments = new List<Appointment>();
    }

    public Appointment Schedule(Patient patient, Doctor doctor, DateTime startsAt, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(doctor);

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        foreach (Appointment current in appointments)
        {
            bool sameDoctor = current.Doctor.Id == doctor.Id;
            bool activeAppointment = !current.IsCancelled;

            if (sameDoctor && activeAppointment && current.Overlaps(startsAt, duration))
            {
                throw new InvalidOperationException(
                    $"Conflito de agenda para {doctor.FullName} em {startsAt:dd/MM/yyyy HH:mm}.");
            }
        }

        Appointment newAppointment = new Appointment(patient, doctor, startsAt, duration);
        appointments.Add(newAppointment);

        return newAppointment;
    }

    public bool CancelAppointment(Guid appointmentId)
    {
        Appointment? appointment = FindById(appointmentId);

        if (appointment is null)
        {
            return false;
        }

        appointment.Cancel();
        return true;
    }

    public IReadOnlyCollection<Appointment> GetDailyAgenda(DateOnly date)
    {
        List<Appointment> result = new List<Appointment>();

        foreach (Appointment appointment in appointments)
        {
            DateOnly appointmentDate = DateOnly.FromDateTime(appointment.StartsAt);

            if (!appointment.IsCancelled && appointmentDate == date)
            {
                result.Add(appointment);
            }
        }

        result.Sort(CompareByStart);
        return result;
    }

    private static int CompareByStart(Appointment left, Appointment right)
    {
        return left.StartsAt.CompareTo(right.StartsAt);
    }

    private Appointment? FindById(Guid appointmentId)
    {
        foreach (Appointment appointment in appointments)
        {
            if (appointment.Id == appointmentId)
            {
                return appointment;
            }
        }

        return null;
    }
}