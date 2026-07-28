using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

if (!OperatingSystem.IsWindows())
{
	Console.WriteLine("Este exemplo funciona somente em Windows, pois usa a API ReadDirectoryChangesW.");
	return;
}

string directoryPath = args.Length > 0 ? args[0] : Environment.CurrentDirectory;

if (!Directory.Exists(directoryPath))
{
	Console.WriteLine("Diretorio nao encontrado: " + directoryPath);
	return;
}

Console.WriteLine("=== ReadDirectoryChangesW Demo ===");
Console.WriteLine("Diretorio monitorado: " + directoryPath);
Console.WriteLine("Subpastas: sim");
Console.WriteLine("Pressione Ctrl+C para encerrar.\n");

using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs eventArgs) =>
{
	eventArgs.Cancel = true;
    if (!cancellationTokenSource.IsCancellationRequested)
    {
        cancellationTokenSource.Cancel();
    }
};

DirectoryChangeMonitor monitor = new DirectoryChangeMonitor(directoryPath);

try
{
	await monitor.RunAsync(cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
	Console.WriteLine("Monitoramento cancelado.");
}

internal sealed class DirectoryChangeMonitor
{
	private const int BufferSize = 16 * 1024;
	private readonly string directoryPath;

	public DirectoryChangeMonitor(string directoryPath)
	{
		this.directoryPath = directoryPath;
	}

	public Task RunAsync(CancellationToken cancellationToken)
	{
		return Task.Run(() => MonitorLoop(cancellationToken), cancellationToken);
	}

	private void MonitorLoop(CancellationToken cancellationToken)
	{
		using SafeFileHandle directoryHandle = NativeMethods.OpenDirectory(directoryPath);
		using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(
			static state => NativeMethods.CancelPendingIo((SafeFileHandle)state!),
			directoryHandle);

		byte[] buffer = new byte[BufferSize];

		while (!cancellationToken.IsCancellationRequested)
		{
			bool success = NativeMethods.ReadDirectoryChanges(directoryHandle, buffer, out uint bytesReturned);

			if (!success)
			{
				int errorCode = Marshal.GetLastPInvokeError();

				if (cancellationToken.IsCancellationRequested && errorCode == NativeMethods.ErrorOperationAborted)
				{
					break;
				}

				throw new Win32Exception(errorCode, "Falha em ReadDirectoryChangesW.");
			}

			if (bytesReturned == 0)
			{
				continue;
			}

			IReadOnlyList<FileChangeNotification> notifications = FileChangeParser.Parse(buffer, bytesReturned);

			foreach (FileChangeNotification notification in notifications)
			{
				Console.WriteLine(
					"["
					+ DateTime.Now.ToString("HH:mm:ss")
					+ "] "
					+ notification.Action
					+ " -> "
					+ notification.FileName);
			}
		}
	}
}

internal static class FileChangeParser
{
	public static IReadOnlyList<FileChangeNotification> Parse(byte[] buffer, uint bytesReturned)
	{
		List<FileChangeNotification> notifications = new List<FileChangeNotification>();
		int offset = 0;
		int totalBytes = checked((int)bytesReturned);

		while (offset < totalBytes)
		{
			if (offset + 12 > totalBytes)
			{
				break;
			}

			uint nextEntryOffset = BitConverter.ToUInt32(buffer, offset);
			uint actionValue = BitConverter.ToUInt32(buffer, offset + 4);
			uint fileNameLength = BitConverter.ToUInt32(buffer, offset + 8);
			int fileNameLengthInBytes = checked((int)fileNameLength);

			if (offset + 12 + fileNameLengthInBytes > totalBytes)
			{
				break;
			}

			string fileName = System.Text.Encoding.Unicode.GetString(buffer, offset + 12, fileNameLengthInBytes);
			FileAction action = Enum.IsDefined(typeof(FileAction), actionValue)
				? (FileAction)actionValue
				: FileAction.Unknown;

			notifications.Add(new FileChangeNotification(action, fileName));

			// FILE_NOTIFY_INFORMATION usa uma lista encadeada por offsets dentro do mesmo buffer.
			if (nextEntryOffset == 0)
			{
				break;
			}

			offset += checked((int)nextEntryOffset);
		}

		return notifications;
	}
}

internal readonly record struct FileChangeNotification(FileAction Action, string FileName);

internal enum FileAction : uint
{
	Added = 0x00000001,
	Removed = 0x00000002,
	Modified = 0x00000003,
	RenamedOldName = 0x00000004,
	RenamedNewName = 0x00000005,
	Unknown = 0xFFFFFFFF
}

internal static class NativeMethods
{
	public const int ErrorOperationAborted = 995;
	private const int ErrorNotFound = 1168;
	private const uint FileListDirectory = 0x0001;
	private const uint FileShareRead = 0x00000001;
	private const uint FileShareWrite = 0x00000002;
	private const uint FileShareDelete = 0x00000004;
	private const uint OpenExisting = 3;
	private const uint FileFlagBackupSemantics = 0x02000000;

	private const uint NotifyFileName = 0x00000001;
	private const uint NotifyDirectoryName = 0x00000002;
	private const uint NotifyAttributes = 0x00000004;
	private const uint NotifySize = 0x00000008;
	private const uint NotifyLastWrite = 0x00000010;
	private const uint NotifyCreation = 0x00000040;

	private const uint NotifyFilter =
		NotifyFileName
		| NotifyDirectoryName
		| NotifyAttributes
		| NotifySize
		| NotifyLastWrite
		| NotifyCreation;

	public static SafeFileHandle OpenDirectory(string directoryPath)
	{
		SafeFileHandle directoryHandle = CreateFileW(
			directoryPath,
			FileListDirectory,
			FileShareRead | FileShareWrite | FileShareDelete,
			IntPtr.Zero,
			OpenExisting,
			FileFlagBackupSemantics,
			IntPtr.Zero);

		if (directoryHandle.IsInvalid)
		{
			int errorCode = Marshal.GetLastPInvokeError();
			directoryHandle.Dispose();
			throw new Win32Exception(errorCode, "Falha ao abrir o diretorio para monitoramento.");
		}

		return directoryHandle;
	}

	public static bool ReadDirectoryChanges(SafeFileHandle directoryHandle, byte[] buffer, out uint bytesReturned)
	{
		return ReadDirectoryChangesW(
			directoryHandle,
			buffer,
			checked((uint)buffer.Length),
			true,
			NotifyFilter,
			out bytesReturned,
			IntPtr.Zero,
			IntPtr.Zero);
	}

	public static void CancelPendingIo(SafeFileHandle directoryHandle)
	{
		bool canceled = CancelIoEx(directoryHandle, IntPtr.Zero);

		if (!canceled)
		{
			int errorCode = Marshal.GetLastPInvokeError();

			if (errorCode != ErrorNotFound && errorCode != ErrorOperationAborted)
			{
				throw new Win32Exception(errorCode, "Falha ao cancelar IO pendente.");
			}
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateFileW(
		string fileName,
		uint desiredAccess,
		uint shareMode,
		IntPtr securityAttributes,
		uint creationDisposition,
		uint flagsAndAttributes,
		IntPtr templateFile);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ReadDirectoryChangesW(
		SafeFileHandle directoryHandle,
		[Out] byte[] buffer,
		uint bufferLength,
		[MarshalAs(UnmanagedType.Bool)] bool watchSubtree,
		uint notifyFilter,
		out uint bytesReturned,
		IntPtr overlapped,
		IntPtr completionRoutine);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CancelIoEx(SafeFileHandle fileHandle, IntPtr overlapped);
}
