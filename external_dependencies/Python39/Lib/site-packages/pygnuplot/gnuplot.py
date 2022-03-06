#!/usr/bin/env python
#coding=utf8
"""
Gnuplot for python
"""

import sys, os, string, types, time
import collections
import subprocess
try:
    # Python 2.
    from StringIO import StringIO
    # Python 3.
except ImportError:
    from io import StringIO
#import numpy as np  
import pandas as pd

def make_plot(*args, **kwargs):

    subplot = {'data': None,
            'subtype': 'plot',
            'cmd': [] }
    subplot["attribute"] = collections.OrderedDict()

    for v in args:
        subplot["cmd"].append(v)
    for k,v in kwargs.items():
        subplot["attribute"][k] = v
    return subplot

def make_splot(*args, **kwargs):

    subplot = {'data': None,
            'subtype': 'splot',
            'cmd': [] }
    subplot["attribute"] = collections.OrderedDict()

    for v in args:
        subplot["cmd"].append(v)
    for k,v in kwargs.items():
        subplot["attribute"][k] = v
    return subplot

def make_plot_data (data, *args, **kwargs):
    subplot = {'data': data,
            'subtype': 'plot',
            'cmd': []}
    subplot["attribute"] = collections.OrderedDict()

    for v in args:
        subplot["cmd"].append(v)
    for k,v in kwargs.items():
        subplot["attribute"][k] = v
    return subplot

def make_splot_data (data, *args, **kwargs):
    subplot = {'data': data,
            'subtype': 'splot',
            'cmd': []}
    subplot["attribute"] = collections.OrderedDict()

    for v in args:
        subplot["cmd"].append(v)
    for k,v in kwargs.items():
        subplot["attribute"][k] = v
    return subplot

def multiplot(*args, **kwargs):
    g = Gnuplot()

    g.set(**kwargs)
    if 'multiplot' not in kwargs.keys():
        g.cmd('set multiplot')

    for subplot in args:

        if (subplot["data"] is not None):
            # Plotting the data, set seperator = ","
            if ('datafile' not in subplot["attribute"].keys()) or \
                ('separator' not in subplot["attribute"]['datafile']):
                g.set(datafile = 'separator ","')

        g.set(**subplot["attribute"])
        c = subplot["subtype"]

        if (subplot["data"] is not None):
            # Convert the data to string format:
            if isinstance(subplot["data"], pd.DataFrame):
                content = subplot["data"].to_csv()
            else:
                content = str(subplot["data"])
            g.__call__('$DataFrame << EOD\n%s\nEOD' %(content))

            for item in subplot["cmd"]:
                c += ' $DataFrame %s,\\\n' %(item)
        else:
            for item in subplot["cmd"]:
                c += ' %s,\\\n' %(item)
        cmd = c.rstrip(",\\\r\n")
        g.cmd(cmd + '\n')
        g.unset('for [i=1:200] label i')
    g.reset()

def plot(*args, **kwargs):
    '''
    *items: The list of plot command;
    **kwargs: The options that would be set before the plot command.
    '''
    __gnuplot("plot", *args, **kwargs)

def splot(*args, **kwargs):
    __gnuplot("splot", *args, **kwargs)

def __gnuplot(plot_cmd, *args, **kwargs):
    g = Gnuplot()
    g.set(**kwargs)
    c = plot_cmd
    for cmd in args:
        c += ' %s,' %(cmd)
    g.cmd(c.rstrip(','))
    g.reset()


def plot_data(data, *args, **kwargs):
    __gnuplot_data(data, "plot", *args, **kwargs)

def splot_data(data, *args, **kwargs):
    __gnuplot_data(data, "splot", *args, **kwargs)

def __gnuplot_data(data, plot_cmd, *args, **kwargs):
    '''
    data: The data that need to be plotted. It's either the string of list
    or the Pnadas Dataframe, if it's Pnadas Dataframe it would be converted
    to string by data.to_csv(). Note that we will execut a extra command
    "set datafile separator "," to fit the data format of csv.
    *items: The list of plot command;
    **kwargs: The options that would be set before the plot command.
    '''

    g = Gnuplot()

    # kwargs input:

    if ('datafile' not in kwargs.keys()) or ('separator' not in kwargs['datafile']):
        g.set(datafile = 'separator ","')

    g.set(**kwargs)

    # Conver the data to string format:
    if isinstance(data, pd.DataFrame):
        content = data.to_csv()
    else:
        content = str(data)
    g.__call__('$DataFrame << EOD\n%s\nEOD' %(content))
    c = plot_cmd
    for cmd in args:
        c += ' $DataFrame %s,' %(cmd)
    #print(c)
    g.cmd(c.rstrip(','))
    g.reset()

class Gnuplot(object):
    """Unsophisticated interface to a running gnuplot program.

    See gp_unix.py for usage information.

    """

    def __init__(self, *args, log = False, **kwargs):
        '''
        *args: The flag parameter in gnuplot
        log: If show the gnuplot log
        **kwargs: the flag that need to be set. You can also set them in the set() function.
        '''

        self.isMultiplot = False
        self.gnuplot = subprocess.Popen(['gnuplot','-p'], shell=True, stdin=subprocess.PIPE)
        # forward write and flush methods:
        self.write = self.gnuplot.stdin.write
        self.flush = self.gnuplot.stdin.flush
        self.log = log

        self.set(*args)
        self.set(**kwargs)

    def __del__(self):
        #print("%s:%d" %(os.path.basename(__file__), sys._getframe().f_lineno))
        self.close()

    def cmd(self, *args):
        '''
        *args: all the line that need to pass to gnuplot. It could be a list of
        lines, or a paragraph; Lines starting with "#" would be omitted. Every
        line should be a clause that could be executed in gnuplot.
        '''
        commands = []
        for cmd in args:
            cmd = filter(lambda x: (x.strip()) and (x.strip()[0] != '#'),
                    StringIO(cmd.strip()).readlines())
            # remove the leading or trailing \r\n
            commands += map(lambda x: x.strip(), cmd)

        for c in commands:
            if self.log:
                #now = time.strftime('%Y%m%d-%H:%M:%S', time.localtime(time.time()))
                now = time.strftime('%H:%M:%S', time.localtime(time.time()))
                print("\033[1;34m[py-gnuplot %s]\033[0m %s" %(now, c))
            self.__call__('%s' %(c))

    def close(self):
        if self.gnuplot is not None:
            self.gnuplot.stdin.write(bytes('quit\n', encoding = "utf8")) #close the gnuplot window
            self.gnuplot = None

    def abort(self):
        if self.gnuplot is not None:
            self.gnuplot.kill()
            self.gnuplot = None

    def cd(self, path):
        self.cmd('cd %s' %(path))

    def call(self, filename, *items):
        params = ""
        for item in items:
            params += " " + item
        self.cmd('call "%s" %s' %(filename, params))

    def clear(self):
        self.cmd('clear')

    def do(self, iteration, *commands):
        self.cmd('do %s {' %(iteration))
        for cmd in commands:
            self.cmd('%s' %(cmd))
        self.cmd('}')

    def set(self, *args, **kwargs):
        '''
        *args: options without value
        *kwargs: options with value. The set and unset commands may optionally
                 contain an iteration clause, so the arg could be list.
        '''
        for v in args:
            self.cmd('set %s' %(v))
        if 'multiplot' in args:
            #print("Enter Multiplot mode.")
            self.isMultiplot = True

        for k,v in kwargs.items():
            if (k == 'multiplot'):
                if (v is not None):
                    #print("Enter Multiplot mode.")
                    self.isMultiplot = True
                else:
                    #print("Exit Multiplot mode.")
                    self.isMultiplot = False
            if isinstance(v, list):
                for i in v:
                    self.cmd('set %s %s' %(k, i))
            else:
                if (v is None):
                    self.cmd('unset %s' %(k))
                else:
                    self.cmd('set %s %s' %(k, v))

    def unset(self, *items):
        '''
        *args: options that need to be unset
        '''
        for item in items:
            self.cmd('unset %s\n' %(item))

    def reset(self):
        self.cmd('reset')

    def plot(self, *items, **kwargs):
        '''
        *items: The list of plot command;
        **kwargs: The options that would be set before the plot command.
        '''
        self.set(**kwargs)
        c = 'plot'
        for item in items:
            c = c + " " + item + ","
        cmd = c.rstrip(',')
        self.cmd(cmd + '\n')

        # unset the label if it's in multiplot mode.
        if self.isMultiplot:
            self.unset('for [i=1:200] label i')

    def plot_data(self, data, *items, **kwargs):
        '''
        data: The data that need to be plotted. It's either the string of list
        or the Pnadas Dataframe, if it's Pnadas Dataframe it would be converted
        to string by data.to_csv(). Note that we will execut a extra command
        "set datafile separator "," to fit the data format of csv.
        *items: The list of plot command;
        **kwargs: The options that would be set before the plot command.
        '''

        if ('datafile' not in kwargs.keys()) or ('separator' not in kwargs['datafile']):
            self.set(datafile = 'separator ","')

        self.set(**kwargs)
        c = 'plot'

        if isinstance(data, pd.DataFrame):
            content = data.to_csv()
        else:
            content = str(data)

        #self.cmd('$DataFrame << EOD\n%s\nEOD' %(content))
        self.__call__('$DataFrame << EOD\n%s\nEOD' %(content))
        for item in items:
            c += ' $DataFrame %s,\\\n' %(item)
        cmd = c.rstrip(",\\\r\n")
        self.cmd(cmd + '\n')

        # unset the label if it's in multiplot mode.
        if self.isMultiplot:
            self.unset('for [i=1:200] label i')

    def splot_data(self, data, *items, **kwargs):
        '''
        data: The data that need to be plotted. It's either the string of list
        or the Pnadas Dataframe, if it's Pnadas Dataframe it would be converted
        to string by data.to_csv(). Note that we will execut a extra command
        "set datafile separator "," to fit the data format of csv.
        *items: The list of plot command;
        **kwargs: The options that would be set before the plot command.
        '''

        if ('datafile' not in kwargs.keys()) or ('separator' not in kwargs['datafile']):
            self.set(datafile = 'separator ","')

        self.set(**kwargs)
        c = 'splot'

        if isinstance(data, pd.DataFrame):
            content = data.to_csv()
        else:
            content = str(data)

        #self.cmd('$DataFrame << EOD\n%s\nEOD' %(content))
        self.__call__('$DataFrame << EOD\n%s\nEOD' %(content))
        for item in items:
            c += ' $DataFrame %s,' %(item)
        cmd = c.rstrip(',')
        self.cmd(cmd + '\n')

        # unset the label if it's in multiplot mode.
        if self.isMultiplot:
            self.unset('for [i=1:200] label i')

    def splot(self, *items, **kwargs):
        self.set(**kwargs)
        c = 'splot'
        for item in items:
            c = c + " " + item + ","
        cmd = c.rstrip(',')
        self.cmd(cmd + '\n')

        # unset the label if it's in multiplot mode.
        if self.isMultiplot:
            self.unset('for [i=1:200] label i')


    def evaluate(self, cmd):
        self.cmd('evaluate %s' %(cmd))

    def exit(self):
        self.cmd('exit')

    def fit(self, cmd):
        #TODO: to be done.
        self.cmd('fit %s' %(cmd))

    def help(self, cmd):
        self.cmd('help %s\r\n' %(cmd))

    def history(self, cmd):
        self.cmd('history %s' %(cmd))

    def load(self, filename):
        self.cmd('load %s' %(cmd))

    def pause(self, param):
        self.cmd('pause %s\n' %(param))

    def __getitem__(self, name): return self.__dict__.get(name.lower(), None)

    def __setitem__(self, name, value):
        self.cmd('set %s %s\n' %(name, value))

    def __call__(self, s):
        """Send a command string to gnuplot, followed by newline."""
        cmd = s + '\n'
        self.write(cmd.encode('utf-8'))
        self.flush()

if __name__ == '__main__':
    g = Gnuplot()
    #ts = pd.Series(np.random.randn(10))
