import subprocess
print(subprocess.check_output("git diff --staged", shell=True).decode('utf-8'))
