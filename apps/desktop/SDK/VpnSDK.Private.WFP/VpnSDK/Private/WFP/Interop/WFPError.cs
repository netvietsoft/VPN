using System.ComponentModel;

namespace VpnSDK.Private.WFP.Interop;

public enum WFPError : uint
{
	[Description("The callout does not exist.")]
	FWP_E_CALLOUT_NOT_FOUND = 2150760449u,
	[Description("The filter condition does not exist.")]
	FWP_E_CONDITION_NOT_FOUND,
	[Description("The filter does not exist.")]
	FWP_E_FILTER_NOT_FOUND,
	[Description("The layer does not exist.")]
	FWP_E_LAYER_NOT_FOUND,
	[Description("The provider does not exist.")]
	FWP_E_PROVIDER_NOT_FOUND,
	[Description("The provider context does not exist.")]
	FWP_E_PROVIDER_CONTEXT_NOT_FOUND,
	[Description("The sub-layer does not exist.")]
	FWP_E_SUBLAYER_NOT_FOUND,
	[Description("The object does not exist.")]
	FWP_E_NOT_FOUND,
	[Description("An object with that GUID or LUID already exists.")]
	FWP_E_ALREADY_EXISTS,
	[Description("The object is referenced by other objects, so it cannot be deleted.")]
	FWP_E_IN_USE,
	[Description("The call is not allowed from within a dynamic session.")]
	FWP_E_DYNAMIC_SESSION_IN_PROGRESS,
	[Description("The call was made from the wrong session, so it cannot be completed.")]
	FWP_E_WRONG_SESSION,
	[Description("The call must be made from within an explicit transaction.")]
	FWP_E_NO_TXN_IN_PROGRESS,
	[Description("The call is not allowed from within an explicit transaction.")]
	FWP_E_TXN_IN_PROGRESS,
	[Description("The explicit transaction has been forcibly canceled.")]
	FWP_E_TXN_ABORTED,
	[Description("The session has been canceled.")]
	FWP_E_SESSION_ABORTED,
	[Description("The call is not allowed from within a read-only transaction.")]
	FWP_E_INCOMPATIBLE_TXN,
	[Description("The call timed out while waiting to acquire the transaction lock.")]
	FWP_E_TIMEOUT,
	[Description("The collection of network diagnostic events is disabled.")]
	FWP_E_NET_EVENTS_DISABLED,
	[Description("The operation is not supported by the specified layer.")]
	FWP_E_INCOMPATIBLE_LAYER,
	[Description("The call is allowed for kernel-mode callers only.")]
	FWP_E_KM_CLIENTS_ONLY,
	[Description("The call tried to associate two objects with incompatible lifetimes.")]
	FWP_E_LIFETIME_MISMATCH,
	[Description("The object is built-in, so it cannot be deleted.")]
	FWP_E_BUILTIN_OBJECT,
	[Description("The maximum number of callouts has been reached.")]
	FWP_E_TOO_MANY_CALLOUTS,
	[Description("A notification could not be delivered because a message queue is at its maximum capacity.")]
	FWP_E_NOTIFICATION_DROPPED,
	[Description("The network traffic parameters do not match those for the security association context.")]
	FWP_E_TRAFFIC_MISMATCH,
	[Description("The call is not allowed for the current security association (SA) state.")]
	FWP_E_INCOMPATIBLE_SA_STATE,
	[Description("A required pointer is null.")]
	FWP_E_NULL_POINTER,
	[Description("An enumerator value in a structure is out of range.")]
	FWP_E_INVALID_ENUMERATOR,
	[Description("The flags field contains an invalid value.")]
	FWP_E_INVALID_FLAGS,
	[Description("A network mask is not valid.")]
	FWP_E_INVALID_NET_MASK,
	[Description("An FWP_RANGE0 structure is not valid.")]
	FWP_E_INVALID_RANGE,
	[Description("The time interval is not valid.")]
	FWP_E_INVALID_INTERVAL,
	[Description("An array that must contain at least one element has zero length.")]
	FWP_E_ZERO_LENGTH_ARRAY,
	[Description("The displayData.name field cannot be null.")]
	FWP_E_NULL_DISPLAY_NAME,
	[Description("The action type is not one of the allowed action types for a filter.")]
	FWP_E_INVALID_ACTION_TYPE,
	[Description("The filter weight is not valid.")]
	FWP_E_INVALID_WEIGHT,
	[Description("A filter condition contains a match type that is not compatible with the operands.")]
	FWP_E_MATCH_TYPE_MISMATCH,
	[Description("An FWP_VALUE0 structure or an FWPM_CONDITION_VALUE0 structure is of the wrong type.")]
	FWP_E_TYPE_MISMATCH,
	[Description("An integer value is outside the allowed range.")]
	FWP_E_OUT_OF_BOUNDS,
	[Description("A reserved field is nonzero.")]
	FWP_E_RESERVED,
	[Description("A filter cannot contain multiple conditions operating on a single field.")]
	FWP_E_DUPLICATE_CONDITION,
	[Description("A policy cannot contain the same keying module more than once.")]
	FWP_E_DUPLICATE_KEYMOD,
	[Description("The action type is not compatible with the layer.")]
	FWP_E_ACTION_INCOMPATIBLE_WITH_LAYER,
	[Description("The action type is not compatible with the sub-layer.")]
	FWP_E_ACTION_INCOMPATIBLE_WITH_SUBLAYER,
	[Description("The raw context or the provider context is not compatible with the layer.")]
	FWP_E_CONTEXT_INCOMPATIBLE_WITH_LAYER,
	[Description("The raw context or the provider context is not compatible with the callout.")]
	FWP_E_CONTEXT_INCOMPATIBLE_WITH_CALLOUT,
	[Description("The authentication method is not compatible with the policy type.")]
	FWP_E_INCOMPATIBLE_AUTH_METHOD,
	[Description("The Diffie-Hellman group is not compatible with the policy type.")]
	FWP_E_INCOMPATIBLE_DH_GROUP,
	[Description("An IKE policy cannot contain an Extended Mode policy.")]
	FWP_E_EM_NOT_SUPPORTED,
	[Description("The enumeration template or subscription will never match any objects.")]
	FWP_E_NEVER_MATCH,
	[Description("The provider context is of the wrong type.")]
	FWP_E_PROVIDER_CONTEXT_MISMATCH,
	[Description("The parameter is incorrect.")]
	FWP_E_INVALID_PARAMETER,
	[Description("The maximum number of sublayers has been reached.")]
	FWP_E_TOO_MANY_SUBLAYERS,
	[Description("The notification function for a callout returned an error.")]
	FWP_E_CALLOUT_NOTIFICATION_FAILED,
	[Description("The IPsec authentication transform is not valid.")]
	FWP_E_INVALID_AUTH_TRANSFORM,
	[Description("The IPsec cipher transform is not valid.")]
	FWP_E_INVALID_CIPHER_TRANSFORM
}
