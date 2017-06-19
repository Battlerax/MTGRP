@echo off

echo Starting compilation 

for /R %%f in (*.js) do (
	node_modules\.bin\javascript-obfuscator %%f --compact true --selfDefending true -o mtgvrp\bin\Debug\resources\%*
)

echo Done!
pause