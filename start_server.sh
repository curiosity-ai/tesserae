cd Tesserae.Tests/bin/Debug/netstandard2.0/h5/
npx http-server -p 8080 > ../../../../../server_output.log 2>&1 &
echo $! > ../../../../../server_pid.txt
