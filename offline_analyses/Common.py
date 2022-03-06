from tabulate import tabulate


## general methods

def PrintStars(n=100):
    print(('*' * (int(n/len('*'))+1))[:n])


def Stop():
    raise Exception("stop!")


def Tabulate(df):
    print(tabulate(df, headers='keys', tablefmt='psql'))