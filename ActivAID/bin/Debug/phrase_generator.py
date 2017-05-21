
from logical_remove_monad import *
#from nltk.probability import *
from nltk import pos_tag
#from nltk.stem import WordNetLemmatizer
#from nltk.stem import LancasterStemmer
from copy import copy
from itertools import combinations
from nltk.parse.util import TestGrammar
import re
import sys

def __valid_sentence(pos, template):
    #print("".join(template), pos)
    return re.compile("".join(template)).match(pos)

def __check_pos(sentence,templates):
    pos = "".join([tup[1] for tup in sentence])
    return any([__valid_sentence(pos, template) for template in templates])

def __get_scores(ngrams_list, keywords):
    score_dict = {}
    for tup in ngrams_list:
        if tup in score_dict:
            score_dict[tup] = score_dict[tup] + 1 + .1 * sum([1 for i in filter(lambda x: x in keywords,tup)])
        else:
            score_dict[tup] = 1
    return score_dict

def __get_max_scores(score_dict):#bingo non 
    check = [("",-99),("",-99),("",-99),("",-99),("",-99)]
    for tup, score in score_dict.items():
        index = 0
        for top5 in check:
            check = list(sorted(check, key=lambda x: x[1]))
            if score > top5[1]:
                break
            index +=1            
        if index < len(check):
            check[index] = tup,score
    return check

def __find_candidates(ngram_list, check):
    candidates = []
    #print(check)
    for j, n in enumerate(ngram_list):
        if n in check:
            candidates.append(j)
    #print(len(candidates))
    return candidates
    
def __find_phrases(lrm_filtered, candidates, templates):
    pos_tagged = []
    for n in candidates:
        try:
            pos_tagged.append(pos_tag(lrm_filtered[n]))
        except:
            continue
    return [n for n in pos_tagged if __check_pos(n,templates)]

#regex " "{2,*} -> " "
def __change_to_lower(text, keywords):
    return text.lower(), list(map(lambda x: x.lower(), keywords))

def __text_to_phrases(text, keywords):
    text, keywords = __change_to_lower(text, keywords)
    only_keyword_tups= resolve_keywords_only_keyword_tups(keywords)
    lrm = LogicalRemoveMonad(text) | remove_punctuation | only_keyword_tups | remove_useless_tups | to_lower
    lrm_filtered = (lrm | interpolate_removed_words).logically_removed[0]
    sentence_templates = [("^VB(S|G)?","(DT|IN|TO)", "NNS?$"),("^NNS?","CC","NNS?$"),
                          ("^NN","TO","VB$"),("^DT","JJ","NN$")]

    check = __get_max_scores(__get_scores(lrm.logically_removed[0], keywords))
    candidates = __find_candidates(lrm.logically_removed[0], dict(check))
    return __find_phrases(lrm_filtered, candidates, sentence_templates)

def gen_phrases(text, keywords):
	for phrase in __text_to_phrases(text, keywords):
		yield phrase[0][0] + " " + phrase[1][0] + " " + phrase[2][0]
