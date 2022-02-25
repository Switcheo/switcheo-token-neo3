
# Test Commands

## List addresses

```bash
list address
#    Address: NUVHSYuSqAHHNpVyeVR2KkggHNiw5DD2nN	Standard
# ScriptHash: 0xca19641b971ec37f99dc098e88861decb9f80d5e
#
#    Address: NMWkRQAjmeLuV9PjPBbFDDLgppV3rXovzn	Standard
# ScriptHash: 0x921fd44af69e335e6659799bcbd6d2616a078c11
#
#    Address: NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6	Standard
# ScriptHash: 0xf035cd377ec59154494d1bd64babeb2901a8cd0a
```

## Add Bridge

```bash
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 addBridge [{"type":"Hash160","value":"921fd44af69e335e6659799bcbd6d2616a078c11"}] NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6
```

## Mint & Transfer

### Mint

```bash
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 transfer [{"type":"Hash160","value":"921fd44af69e335e6659799bcbd6d2616a078c11"},{"type":"Hash160","value":"f035cd377ec59154494d1bd64babeb2901a8cd0a"},{"type":"Integer","value":"4200000000"},{"type":"Boolean","value":false}] NUVHSYuSqAHHNpVyeVR2KkggHNiw5DD2nN NMWkRQAjmeLuV9PjPBbFDDLgppV3rXovzn
```

### Transfer to Owner

```bash
transfer 0x285b332bc0323bc334987bd4735fb39cc3269e20 NUVHSYuSqAHHNpVyeVR2KkggHNiw5DD2nN 8 NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6
```

### Transfer to Bridge

```bash
transfer 0x285b332bc0323bc334987bd4735fb39cc3269e20 NMWkRQAjmeLuV9PjPBbFDDLgppV3rXovzn 10 NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6
```

## Transfer Ownership

### Initiate transfer

```bash
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 initiateOwnershipTransfer [{"type":"Hash160","value":"f035cd377ec59154494d1bd64babeb2901a8cd0a"}] NUVHSYuSqAHHNpVyeVR2KkggHNiw5DD2nN
```

### Accept transfer

```bash
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 acceptOwnershipTransfer [] NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6

# check owner
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 getOwner
# => Result Stack: [{"type":"ByteString","value":"Cs2oASnrq0vWG01JVJHFfjfNNfA="}]
parse "Cs2oASnrq0vWG01JVJHFfjfNNfA="

# check pending owner
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 getPendingOwner
# => Result Stack: [{"type":"Any"}]
```

## Remove Bridge

```bash
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 removeBridge [{"type":"Hash160","value":"921fd44af69e335e6659799bcbd6d2616a078c11"}] NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6

# check minting fails
invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 transfer [{"type":"Hash160","value":"921fd44af69e335e6659799bcbd6d2616a078c11"},{"type":"Hash160","value":"f035cd377ec59154494d1bd64babeb2901a8cd0a"},{"type":"Integer","value":"4200000000"},{"type":"Boolean","value":false}] NUVHSYuSqAHHNpVyeVR2KkggHNiw5DD2nN NMWkRQAjmeLuV9PjPBbFDDLgppV3rXovzn
# => An unhandled exception was thrown. Insufficient balance.

# re-add and mint

invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 addBridge [{"type":"Hash160","value":"921fd44af69e335e6659799bcbd6d2616a078c11"}] NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6

invoke 0x285b332bc0323bc334987bd4735fb39cc3269e20 transfer [{"type":"Hash160","value":"921fd44af69e335e6659799bcbd6d2616a078c11"},{"type":"Hash160","value":"f035cd377ec59154494d1bd64babeb2901a8cd0a"},{"type":"Integer","value":"1000000000000"},{"type":"Boolean","value":false}] NLu6JvSgqz8h3wobdiK5ZGuoMLSQeLoNs6 NMWkRQAjmeLuV9PjPBbFDDLgppV3rXovzn
```
