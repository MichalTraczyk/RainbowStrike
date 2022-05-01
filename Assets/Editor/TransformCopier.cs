using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;

public class TransformCopier : ScriptableObject
{

	public enum MethodID
	{
		MethodNone,
		MethodCopyTransformToClipboard,
		MethodCopyGlobalPosition,
		MethodPasteGlobalPosition,
		MethodCopyLocalPosition,
		MethodPasteLocalPosition,
		MethodCopyGlobalRotation,
		MethodPasteGlobalRotation,
		MethodCopyLocalRotation,
		MethodPasteLocalRotation,
	};

	static MethodID recentlyExecutedMethodsID = MethodID.MethodNone;

	static Vector3 globalPosition;
	static Vector3 localPosition;
	static Quaternion globalRotation;
	static Quaternion localRotation;

	static Vector3 tempPositionForUndoAction;
	static Quaternion tempRotationForUndoAction;
	static Transform undoTargetTransform;

	[MenuItem("Tools/TransformCopier/DoPasteForLastCopy")]
	public static void PasteActionForLastCopy()
	{
		switch (recentlyExecutedMethodsID)
		{
			case MethodID.MethodCopyGlobalPosition: PasteGlobalPosition(); break;
			case MethodID.MethodCopyLocalPosition: PasteLocalPosition(); break;
			case MethodID.MethodCopyGlobalRotation: PasteGlobalRotation(); break;
			case MethodID.MethodCopyLocalRotation: PasteLocalRotation(); break;
			default: break;
		}
	}

	[MenuItem("Tools/TransformCopier/CopyGlobalPosition")]
	public static void CopyGlobalPositionMainMenu()
	{
		CopyGlobalPosition();
	}

	[MenuItem("Tools/TransformCopier/PasteGlobalPosition")]
	public static void PasteGlobalPositionMainMenu()
	{
		PasteGlobalPosition();
	}

	[MenuItem("Tools/TransformCopier/CopyGlobalRotation")]
	public static void CopyGlobalRotationMainMenu()
	{
		CopyGlobalRotation();
	}

	[MenuItem("Tools/TransformCopier/PasteGlobalRotation")]
	public static void PasteGlobalRotationMainMenu()
	{
		PasteGlobalRotation();
	}

	[MenuItem("Tools/TransformCopier/Position/Global/Copy")]
	public static void CopyGlobalPosition()
	{
		globalPosition = Selection.activeTransform.position;
		recentlyExecutedMethodsID = MethodID.MethodCopyGlobalPosition;
	}

	[MenuItem("Tools/TransformCopier/Position/Global/Paste")]
	public static void PasteGlobalPosition()
	{
		tempPositionForUndoAction = Selection.activeTransform.position;
		undoTargetTransform = Selection.activeTransform;

		Selection.activeTransform.position = globalPosition;

		recentlyExecutedMethodsID = MethodID.MethodPasteGlobalPosition;
	}

	[MenuItem("Tools/TransformCopier/Position/Local/Copy")]
	public static void CopyLocalPosition()
	{
		localPosition = Selection.activeTransform.localPosition;
		recentlyExecutedMethodsID = MethodID.MethodCopyLocalPosition;
	}

	[MenuItem("Tools/TransformCopier/Position/Local/Paste")]
	public static void PasteLocalPosition()
	{
		tempPositionForUndoAction = Selection.activeTransform.localPosition;
		undoTargetTransform = Selection.activeTransform;

		Selection.activeTransform.localPosition = localPosition;
		recentlyExecutedMethodsID = MethodID.MethodPasteLocalPosition;
	}

	[MenuItem("Tools/TransformCopier/Rotation/Global/Copy")]
	public static void CopyGlobalRotation()
	{
		globalRotation = Selection.activeTransform.rotation;
		recentlyExecutedMethodsID = MethodID.MethodCopyGlobalRotation;
	}

	[MenuItem("Tools/TransformCopier/Rotation/Global/Paste")]
	public static void PasteGlobalRotation()
	{
		tempRotationForUndoAction = Selection.activeTransform.rotation;
		undoTargetTransform = Selection.activeTransform;

		Selection.activeTransform.rotation = globalRotation;
		recentlyExecutedMethodsID = MethodID.MethodPasteGlobalRotation;
	}

	[MenuItem("Tools/TransformCopier/Rotation/Local/Copy")]
	public static void CopyLocalRotation()
	{
		localRotation = Selection.activeTransform.localRotation;
		recentlyExecutedMethodsID = MethodID.MethodCopyLocalRotation;
	}

	[MenuItem("Tools/TransformCopier/Rotation/Local/Paste")]
	public static void PasteLocalRotation()
	{
		tempRotationForUndoAction = Selection.activeTransform.localRotation;
		undoTargetTransform = Selection.activeTransform;

		Selection.activeTransform.localRotation = localRotation;
		recentlyExecutedMethodsID = MethodID.MethodPasteLocalRotation;
	}

	[MenuItem("Tools/TransformCopier/RepeatLastAction")]
	public static void RepeatLastAction()
	{
		switch (recentlyExecutedMethodsID)
		{
			case MethodID.MethodCopyGlobalPosition: CopyGlobalPosition(); break;
			case MethodID.MethodPasteGlobalPosition: PasteGlobalPosition(); break;
			case MethodID.MethodCopyLocalPosition: CopyLocalPosition(); break;
			case MethodID.MethodPasteLocalPosition: PasteLocalPosition(); break;
			case MethodID.MethodCopyGlobalRotation: CopyGlobalRotation(); break;
			case MethodID.MethodPasteGlobalRotation: PasteGlobalRotation(); break;
			case MethodID.MethodCopyLocalRotation: CopyLocalRotation(); break;
			case MethodID.MethodPasteLocalRotation: PasteLocalPosition(); break;
			case MethodID.MethodCopyTransformToClipboard: CopyTransformToClipboard(); break;
			default: break;
		}
	}

	[MenuItem("Tools/TransformCopier/Undo")]
	public static void Undo()
	{
		switch (recentlyExecutedMethodsID)
		{
			case MethodID.MethodPasteGlobalPosition:
				undoTargetTransform.position = tempPositionForUndoAction;
				break;
			case MethodID.MethodPasteLocalPosition:
				undoTargetTransform.localPosition = tempPositionForUndoAction;
				break;
			case MethodID.MethodPasteGlobalRotation:
				undoTargetTransform.rotation = tempRotationForUndoAction;
				break;
			case MethodID.MethodPasteLocalRotation:
				undoTargetTransform.localRotation = tempRotationForUndoAction;
				break;

		}
	}


	private static string Vector3ToString(Vector3 v)
	{
		return v.ToString("f4");
	}

	private static string QuaternionToString(Quaternion q)
	{
		return q.ToString("f4");
	}


	[MenuItem("Tools/TransformCopier/CopyTransformToClipboard &%c")]
	public static void CopyTransformToClipboard()
	{
		Transform activeTransform = Selection.activeTransform;

		StringBuilder stringBuilder = new StringBuilder();

		stringBuilder.AppendFormat("{0}", activeTransform.gameObject.name);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("{0}{1}", "GlobalPosition: ", Vector3ToString(activeTransform.position));
		stringBuilder.AppendLine();

		stringBuilder.AppendFormat("{0}{1}", "GlobalRotationEulerAngles: ", Vector3ToString(activeTransform.rotation.eulerAngles));
		stringBuilder.AppendLine();

		stringBuilder.AppendFormat("{0}{1}", "GlobalRotationQuaternion: ", QuaternionToString(activeTransform.rotation));
		stringBuilder.AppendLine();

		stringBuilder.AppendFormat("{0}{1}", "GlobalScale: ", Vector3ToString(activeTransform.lossyScale));
		stringBuilder.AppendLine();

		//
		stringBuilder.AppendFormat("{0}{1}", "LocalPosition: ", Vector3ToString(activeTransform.localPosition));
		stringBuilder.AppendLine();

		stringBuilder.AppendFormat("{0}{1}", "LocalRotationEulerAngles: ", Vector3ToString(activeTransform.localRotation.eulerAngles));
		stringBuilder.AppendLine();

		stringBuilder.AppendFormat("{0}{1}", "LocalRotationQuaternion: ", QuaternionToString(activeTransform.localRotation));
		stringBuilder.AppendLine();

		stringBuilder.AppendFormat("{0}{1}", "LocalScale: ", Vector3ToString(activeTransform.localScale));
		stringBuilder.AppendLine();


		EditorGUIUtility.systemCopyBuffer = stringBuilder.ToString();
		recentlyExecutedMethodsID = MethodID.MethodCopyTransformToClipboard;
	}
}