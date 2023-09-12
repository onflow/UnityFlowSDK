flow accounts create --key 3e832723c3c03a025d2c98f953504c7d09407c7a41febfaf4a222f3734aa28d5a62bd0ebc6e9c3d1424ea23675ef86e970284f5dcb36e8eb9a6f45bd21c29603 --signer emulator-account -y

flow accounts create --key 9892824ac9abec70454cb3f6f9641ce7d4f6ae6cb664234d605f8e92e26e2b8aee22094a9da3a2149fc537173b8574d8ab03e27af90838d166cdedc577eab3de --signer emulator-account -y

flow accounts create --key c275cbd96228531890deaabf133aadc5b2c543760aeac316194d43a29d79aa70dd490562aa64f634fec1d48182dedc83f1823acdcdbfa21c309ffdead4446fcb --signer emulator-account -y

flow.exe transactions build .\transactions\transfer-flow-tokens.cdc 0xf3fcd2c1a78f5eee 1000.0 --authorizer emulator-account --proposer emulator-account --payer emulator-account --filter payload --save built.rlp -y

flow.exe transactions sign .\built.rlp --signer emulator-account --filter payload --save signed.rlp -y

flow.exe transactions send-signed .\signed.rlp -y

flow project deploy -y