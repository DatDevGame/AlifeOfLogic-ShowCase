using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MultiplayerDataHelper{
	public enum MessageType
	{
		PuzzleSelect,
		PuzzleLoaded,
		CellValueSet,
		PuzzleSolved,
		SessionFinished,
		Ready,
        SendPlayerInformation,
	}
	public enum MatchResult
	{
		WIN,
		LOSE
	}

    public const int byteSizeConst = 100;
    public static int ByteSizeConst = byteSizeConst;

    public struct PlayerInformationStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = byteSizeConst)]
        public byte[] playerName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = byteSizeConst)]
        public byte[] avatarUrl;
        public short playerNode;
        public short winNumber;
        public short loseNumber;
		public short randomSeed;
    }

    public struct PuzzleSelectStruct
	{
		public short offset;
        public short skinIndex;
	}
	public struct OnCellValueSetStruct
	{
		public short col;
		public short row;
		public short value;
        public short skinIndex;
	}
	public struct OnPuzzleSolvedStruct
	{
		public double timeStamp;
	}

	public T[] SubArray<T>(T[] data, int index, int length)
	{
		T[] result = new T[length];
		Array.Copy(data, index, result, 0, length);
		return result;
	}

	public T fromBytes<T>(byte[] arr, T containerObject)
	{
		int size = Marshal.SizeOf(containerObject);
		IntPtr ptr = Marshal.AllocHGlobal(size);

		Marshal.Copy(arr, 0, ptr, size);

		containerObject = (T) Marshal.PtrToStructure(ptr, containerObject.GetType());
		Marshal.FreeHGlobal(ptr);

		return containerObject;
	}

	public byte[] getBytes(object str)
	{
		int size = Marshal.SizeOf(str);
		byte[] arr = new byte[size];

		IntPtr ptr = Marshal.AllocHGlobal(size);
		Marshal.StructureToPtr(str, ptr, true);
		Marshal.Copy(ptr, arr, 0, size);
		Marshal.FreeHGlobal(ptr);
		return arr;
	}
}
