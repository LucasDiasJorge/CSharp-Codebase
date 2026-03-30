using AssociationMedicalScheduling.Models;
using AssociationMedicalScheduling.Services;

Console.WriteLine("Association - Agenda Medica");
Console.WriteLine(new string('=', 40));

List<Patient> patients = new List<Patient>
{
    new Patient(1, "Aline Costa", new DateOnly(1992, 8, 3)),
    new Patient(2, "Bruno Alves", new DateOnly(1987, 2, 14)),
    new Patient(3, "Camila Rocha", new DateOnly(2001, 11, 5))
};

List<Doctor> doctors = new List<Doctor>
{
    new Doctor(10, "Fernando Lima", "Clinica Geral"),
    new Doctor(11, "Gabriela Nunes", "Cardiologia")
};

AppointmentBook appointmentBook = new AppointmentBook();

DateTime workDay = DateTime.Today.AddDays(1).Date.AddHours(9);

Appointment firstAppointment = appointmentBook.Schedule(
    patients[0],
    doctors[0],
    workDay,
    TimeSpan.FromMinutes(30));

appointmentBook.Schedule(
    patients[1],
    doctors[0],
    workDay.AddMinutes(30),
    TimeSpan.FromMinutes(30));

appointmentBook.Schedule(
    patients[2],
    doctors[1],
    workDay.AddHours(1),
    TimeSpan.FromMinutes(45));

try
{
    appointmentBook.Schedule(
        patients[2],
        doctors[0],
        workDay.AddMinutes(15),
        TimeSpan.FromMinutes(20));
}
catch (InvalidOperationException exception)
{
    Console.WriteLine($"Regra de negocio aplicada: {exception.Message}");
}

PrintDailyAgenda(appointmentBook, DateOnly.FromDateTime(workDay));

appointmentBook.CancelAppointment(firstAppointment.Id);
Console.WriteLine();
Console.WriteLine("Consulta da Aline foi cancelada.");
PrintDailyAgenda(appointmentBook, DateOnly.FromDateTime(workDay));

Console.WriteLine();
Console.WriteLine("Independencia dos objetos (Association):");
Console.WriteLine($"Pacientes cadastrados: {patients.Count}");
Console.WriteLine($"Medicos ativos: {doctors.Count}");
Console.WriteLine("Mesmo com cancelamento de consulta, paciente e medico continuam existindo.");

static void PrintDailyAgenda(AppointmentBook appointmentBook, DateOnly date)
{
    IReadOnlyCollection<Appointment> agenda = appointmentBook.GetDailyAgenda(date);

    Console.WriteLine();
    Console.WriteLine($"Agenda do dia {date:dd/MM/yyyy}");

    if (agenda.Count == 0)
    {
        Console.WriteLine("- Sem consultas ativas para o dia.");
        return;
    }

    foreach (Appointment appointment in agenda)
    {
        Console.WriteLine($"- {appointment}");
    }
}