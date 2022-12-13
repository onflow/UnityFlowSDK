#!/bin/sh

flow accounts create --key 3e832723c3c03a025d2c98f953504c7d09407c7a41febfaf4a222f3734aa28d5a62bd0ebc6e9c3d1424ea23675ef86e970284f5dcb36e8eb9a6f45bd21c29603 --signer emulator-account -y
flow accounts create --key 9892824ac9abec70454cb3f6f9641ce7d4f6ae6cb664234d605f8e92e26e2b8aee22094a9da3a2149fc537173b8574d8ab03e27af90838d166cdedc577eab3de --signer emulator-account -y
flow accounts create --key c275cbd96228531890deaabf133aadc5b2c543760aeac316194d43a29d79aa70dd490562aa64f634fec1d48182dedc83f1823acdcdbfa21c309ffdead4446fcb --signer emulator-account -y
flow project deploy -y
flow transactions build ./transactions/account-init.cdc --authorizer user1 --proposer user1 --payer user1 --filter payload --save built.rlp -y
flow transactions sign ./built.rlp --signer user1 --filter payload --save signed.rlp -y
flow transactions send-signed ./signed.rlp -y
flow transactions build ./transactions/account-init.cdc --authorizer user2 --proposer user2 --payer user2 --filter payload --save built.rlp -y
flow transactions sign ./built.rlp --signer user2 --filter payload --save signed.rlp -y
flow transactions send-signed ./signed.rlp -y
flow transactions build ./transactions/mint-nfts.cdc 0x01cf0e2f2f715450 0x179b6b1cb6755e31 --authorizer admin1 --proposer admin1 --payer admin1 --filter payload --save built.rlp -y
flow transactions sign ./built.rlp --signer admin1 --filter payload --save signed.rlp -y
flow transactions send-signed ./signed.rlp -y
flow transactions build ./transactions/mint-tokens.cdc 0x01cf0e2f2f715450 0x179b6b1cb6755e31 --authorizer admin1 --proposer admin1 --payer admin1 --filter payload --save built.rlp -y
flow transactions sign ./built.rlp --signer admin1 --filter payload --save signed.rlp -y
flow transactions send-signed ./signed.rlp -y