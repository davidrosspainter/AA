import os

# adjusts path depending on whether Python is running from the PyCharm IDE or a shell

isRunningInPyCharm = "PYCHARM_HOSTED" in os.environ

if not isRunningInPyCharm:
        import pathlib
        print(os.path.dirname(os.path.abspath(__file__)))
        os.chdir(os.path.dirname(os.path.abspath(__file__)))  # changes path to use relative paths if run externally