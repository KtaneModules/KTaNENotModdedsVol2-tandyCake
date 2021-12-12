using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NotNumberPad
{
	public class Flash : IEnumerable<NNPButton> {

        private List<NNPButton> _buttons = new List<NNPButton>();
        public IEnumerator<NNPButton> GetEnumerator()
        { return _buttons.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }

		public Flash(IEnumerable<NNPButton> buttons)
        {
            _buttons = new List<NNPButton> (buttons);
        }
        public NNPButton this[int i]
        {
            get { return _buttons[i]; }
        }

        public int[] GetNumbers()
        {
            return _buttons.Select(x => x.value.Value).ToArray();
        }
        public int GetValue(int[] priorities)
        {
            int val = 0;
            int[] nums = GetNumbers();
            foreach (int num in priorities)
                if (nums.Contains(num))
                    val = val * 10 + num;
            return val;
        }
        public override string ToString()
        {
            var buttonCols = _buttons.Select(x => x.color);
            string rtn = "";
            for (int i = 0; i < 4; i++)
                if (buttonCols.Contains((ButtonColor)i))
                    rtn += ((ButtonColor)i).ToString()[0];
            return rtn;
        }
    }

}
