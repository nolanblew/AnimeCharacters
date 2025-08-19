import sys, datetime, pathlib

path = pathlib.Path(sys.argv[1])
try:
    build = int(path.read_text())
except Exception:
    build = 0
build += 1
path.write_text(str(build))
print(datetime.datetime.utcnow().strftime('%Y-%m') + '-' + f'{build:04d}')
