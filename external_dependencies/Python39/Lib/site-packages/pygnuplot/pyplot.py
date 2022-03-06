#!/usr/bin/env python3
#coding=utf8
"""
Gnuplot for python
"""

import sys, os, string, types
from pygnuplot import gnuplot
import collections
import subprocess
#import numpy as np  
import pandas as pd

def test():
    print("gplfinance.test()")

def make_splot(data, *args, **kwargs):
    subplot = {'data': data,
            'subtype': 'splot',
            'cmd': []}
    subplot["attribute"] = collections.OrderedDict()

    for v in args:
        subplot["cmd"].append(v)
    for k,v in kwargs.items():
        subplot["attribute"][k] = v
    return subplot

def make_plot(data, *args, **kwargs):
    subplot = {'data': data,
            'subtype': 'plot',
            'cmd': []}
    subplot["attribute"] = collections.OrderedDict()

    for v in args:
        subplot["cmd"].append(v)
    for k,v in kwargs.items():
        subplot["attribute"][k] = v
    return subplot

def multiplot(*args, **kwargs):
    g = gnuplot.Gnuplot()

    g.set(**kwargs)
    if 'multiplot' not in kwargs.keys():
        g.cmd('set multiplot')

    for subplot in args:
        g.set(**subplot["attribute"])
        #for k,v in subplot["attribute"].items():
        #    if isinstance(v, list):
        #        for i in v:
        #            #print('set %s %s' %(k, i))
        #            g.cmd('set %s %s' %(k, i))
        #    else:
        #        #print('set %s %s' %(k, v))
        #        g.cmd('set %s %s' %(k, v))
        cmd = subplot["cmd"]

        # Conver the data to string format:
        if isinstance(subplot["data"], pd.DataFrame):
            content = subplot["data"].to_csv(sep = ' ')
        else:
            content = str(subplot["data"])
        g.__call__('$DataFrame << EOD\n%s\nEOD' %(content))
        c = subplot["subtype"]
        for cmd in subplot["cmd"]:
            c += ' $DataFrame %s, ' %(cmd)
        #print(c)
        g.cmd(c)
        # multiplot automatically unset the following setting:
        g.unset('for [i=1:200] label i')
        #g.unset('for [i=1:200] label i',
        #        'title',
        #        'xtics', 'x2tics', 'ytics', 'y2tics', 'ztics', 'cbtics',
        #        'xlabel', 'x2label', 'ylabel', 'y2label', 'zlabel', 'cblabel',
        #        'xrange', 'x2range', 'yrange', 'y2range', 'zrange', 'cbrange',
        #        'rrange', 'trange', 'urange', 'vrange')
    g.reset()

def plot(data, *args, **kwargs):
    __gnuplot(data, "plot", *args, **kwargs)

def splot(data, *args, **kwargs):
    __gnuplot(data, "splot", *args, **kwargs)

def __gnuplot(data, plot_cmd, *args, **kwargs):
    g = gnuplot.Gnuplot()

    # kwargs input:
    g.set(**kwargs)

    # Conver the data to string format:
    if isinstance(data, pd.DataFrame):
        content = data.to_csv(sep = ' ')
    else:
        content = str(data)
    g.__call__('$DataFrame << EOD\n%s\nEOD' %(content))
    c = plot_cmd
    for cmd in args:
        c += ' $DataFrame %s,' %(cmd)
    #print(c)
    g.cmd(c.rstrip(','))
    g.reset()

class Gnuplot(gnuplot.Gnuplot):
    """Unsophisticated interface to a running gnuplot program.

    See gp_unix.py for usage information.

    """

    def plot(self, data, *items, **kwargs):
        '''
        data: The data that need to be plotted. It's either the string of list
        or the Pnadas Dataframe, if it's Pnadas Dataframe it would be converted
        to string by data.to_csv(sep = ' ')
        *items: The list of plot command;
        **kwargs: The options that would be set before the plot command.
        '''
        self.set(**kwargs)
        c = 'plot'
        for item in items:
            c = c + ' "-" ' + item + ","
        cmd = c.rstrip(',')
        self.cmd(cmd + '\n')

        if isinstance(data, pd.DataFrame):
            content = data.to_csv(sep = ' ')
        else:
            content = str(data)

        for item in items:
            self.__call__(content)
            self.__call__('e')

    def splot(self, data, *items, **kwargs):
        self.set(**kwargs)
        c = 'splot'
        for item in items:
            c = c + ' "-" ' + item + ","
        cmd = c.rstrip(',')
        self.cmd(cmd + '\n')

        if isinstance(data, pd.DataFrame):
            content = data.to_csv(sep = ' ')
        else:
            content = str(data)

        for item in items:
            self.__call__(content)
            self.__call__('e')

if __name__ == '__main__':
    g = Gnuplot()
    #ts = pd.Series(np.random.randn(10))
